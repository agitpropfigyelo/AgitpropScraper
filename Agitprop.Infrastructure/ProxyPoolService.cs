using System;

namespace Agitprop.Infrastructure;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.Json;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class ProxyPoolService : IProxyPool
{
    private readonly ILogger<ProxyPoolService>? _logger;
    private readonly IConfiguration _config;
    private readonly HttpClient _http = new();
    private readonly ConcurrentDictionary<string, ProxyEntry> _proxies = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly PeriodicTimer _timer;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private readonly Uri _sourceUri;
    private readonly Uri _validateUri;
    private readonly int _validationTimeoutSec;
    private readonly int _validationParallelism;
    private readonly int _validationIntervalMin;
    private readonly int _maxCachedProxies;

    private readonly int _maxConnectionsPerServer;
    private readonly TimeSpan _pooledConnectionLifetime;
    private readonly TimeSpan _pooledConnectionIdleTimeout;
    private readonly TimeSpan _requestTimeout;

    public ProxyPoolService(ILogger<ProxyPoolService>? logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        _sourceUri = new Uri(_config.GetValue<string>("Proxy:SourceUrl","https://cdn.jsdelivr.net/gh/proxifly/free-proxy-list@main/proxies/protocols/https/data.json"));
        _validateUri = new Uri(_config.GetValue<string>("Proxy:ValidateEndpoint", "https://vanenet.hu/"));
        _validationTimeoutSec = _config.GetValue<int>("Proxy:ValidationTimeoutSeconds", 3);
        _validationParallelism = _config.GetValue<int>("Proxy:ValidationParallelism", 50);
        _validationIntervalMin = _config.GetValue<int>("Proxy:ValidationIntervalMinutes", 3);
        _maxCachedProxies = _config.GetValue<int>("Proxy:MaxCachedProxies", 2000);

        _maxConnectionsPerServer = _config.GetValue<int>("HttpClientDefaults:MaxConnectionsPerServer", 100);
        _pooledConnectionLifetime = TimeSpan.FromSeconds(_config.GetValue<int>("HttpClientDefaults:PooledConnectionLifetimeSeconds", 30));
        _pooledConnectionIdleTimeout = TimeSpan.FromSeconds(_config.GetValue<int>("HttpClientDefaults:PooledConnectionIdleTimeoutSeconds", 30));
        _requestTimeout = TimeSpan.FromSeconds(_config.GetValue<int>("HttpClientDefaults:RequestTimeoutSeconds", 15));

        _timer = new PeriodicTimer(TimeSpan.FromMinutes(_validationIntervalMin));

        // Fire-and-forget background refresh loop
        _ = BackgroundLoopAsync(_cts.Token);
    }

    private record ProxyDto(string ip, int port);

    private async Task<IEnumerable<string>> FetchProxyAddressesAsync()
    {
        var res = await _http.GetAsync(_sourceUri, HttpCompletionOption.ResponseHeadersRead);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<ProxyDto>>(json);
        return list?.Select(p => $"{p.ip}:{p.port}") ?? Enumerable.Empty<string>();
    }

    private async Task BackgroundLoopAsync(CancellationToken ct)
    {
        try
        {
            // initial load
            await RefreshAsync(ct);
            while (await _timer.WaitForNextTickAsync(ct))
            {
                await RefreshAsync(ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ProxyPoolService background loop failed");
        }
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        if (!await _refreshLock.WaitAsync(0, ct)) return; // skip if already running
        try
        {
            _logger?.LogInformation("Refreshing proxy list from {url}", _sourceUri);

            var addresses = (await FetchProxyAddressesAsync()).Take(_maxCachedProxies).ToList();

            // Add new entries (do not overwrite existing entries to keep clients alive)
            foreach (var addr in addresses)
            {
                _proxies.GetOrAdd(addr, a => new ProxyEntry(a, CreateInvokerForProxy(a)));
            }

            // Validate existing proxies concurrently, but bounded
            var toValidate = _proxies.Values.ToList();
            var throttler = new SemaphoreSlim(_validationParallelism);
            var tasks = toValidate.Select(async entry =>
            {
                await throttler.WaitAsync(ct);
                try
                {
                    var alive = await ValidateProxyAsync(entry.Address, ct);
                    entry.IsAlive = alive;
                    entry.LastCheckedUtc = DateTime.UtcNow;
                    if (!alive)
                    {
                        // if dead, optionally dispose its invoker to free sockets
                        entry.DisposeInvoker();
                    }
                }
                finally
                {
                    throttler.Release();
                }
            }).ToArray();

            await Task.WhenAll(tasks);

            // Remove too-old / dead entries if pool too big
            var deadKeys = _proxies.Where(kv => !kv.Value.IsAlive && (DateTime.UtcNow - kv.Value.LastCheckedUtc).TotalMinutes > 10)
                                   .Select(kv => kv.Key).ToList();

            foreach (var k in deadKeys)
            {
                if (_proxies.TryRemove(k, out var removed))
                {
                    removed.DisposeInvoker();
                }
            }

            _logger?.LogInformation("Proxy refresh complete: total cached {count}, alive {aliveCount}",
                _proxies.Count, _proxies.Count(kv => kv.Value.IsAlive));
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private HttpMessageInvoker CreateInvokerForProxy(string addr)
    {
        // addr = "ip:port"
        var parts = addr.Split(':');
        var host = parts[0];
        var port = int.Parse(parts[1]);

        var handler = new SocketsHttpHandler
        {
            UseProxy = true,
            Proxy = new WebProxy(host, port),
            PooledConnectionLifetime = _pooledConnectionLifetime,
            PooledConnectionIdleTimeout = _pooledConnectionIdleTimeout,
            MaxConnectionsPerServer = _maxConnectionsPerServer,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ConnectTimeout = TimeSpan.FromSeconds(_validationTimeoutSec)
        };

        // Create invoker (wrapper over handler) so we can reuse it and dispose handler later
        return new HttpMessageInvoker(handler, disposeHandler: true);
    }

    private async Task<bool> ValidateProxyAsync(string addr, CancellationToken ct)
    {
        // quick HEAD/GET to validation endpoint with short timeout
        try
        {
            using var invoker = _proxies.TryGetValue(addr, out var e) && e.Invoker != null
                ? e.Invoker // reuse existing invoker if present (no disposal)
                : CreateInvokerForProxy(addr);

            using var req = new HttpRequestMessage(HttpMethod.Get, _validateUri);
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; ProxyValidator/1.0)");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_validationTimeoutSec));
            var resp = await invoker.SendAsync(req, cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(HttpMessageInvoker? invoker, string address)?> GetRandomInvokerAsync(CancellationToken ct = default)
    {
        // Try to pick a random alive invoker
        var alive = _proxies.Values.Where(e => e.IsAlive && e.Invoker != null).ToArray();
        if (alive.Length == 0)
        {
            // fallback: return any invoker (even if not validated) or null
            var any = _proxies.Values.Where(e => e.Invoker != null).ToArray();
            if (any.Length == 0) return null;
            var pick = any[new Random().Next(any.Length)];
            return (pick.Invoker,pick.Address);
        }

        var selected = alive[new Random().Next(alive.Length)];
        return (selected.Invoker,selected.Address);
    }

    public Task<IEnumerable<string>> GetAliveProxyAddressesAsync() =>
        Task.FromResult(_proxies.Where(kv => kv.Value.IsAlive).Select(kv => kv.Key).AsEnumerable());

    public Task MarkDeadAsync(string proxyAddress)
    {
        if (_proxies.TryGetValue(proxyAddress, out var entry))
        {
            entry.IsAlive = false;
            entry.LastCheckedUtc = DateTime.UtcNow;
            entry.DisposeInvoker();
        }
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _timer.Dispose();
        foreach (var kv in _proxies) kv.Value.DisposeInvoker();
        _refreshLock.Dispose();
        _http.Dispose();
        _cts.Dispose();
        await Task.CompletedTask;
    }

    private class ProxyEntry
    {
        public string Address { get; }
        public HttpMessageInvoker? Invoker { get; private set; }
        public bool IsAlive { get; set; } = false;
        public DateTime LastCheckedUtc { get; set; } = DateTime.MinValue;

        public ProxyEntry(string addr, HttpMessageInvoker inv)
        {
            Address = addr;
            Invoker = inv;
        }

        public void DisposeInvoker()
        {
            try
            {
                Invoker?.Dispose();
            }
            catch { }
            Invoker = null;
        }
    }
}
