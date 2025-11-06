namespace Agitprop.Infrastructure;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Diagnostics;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Agitprop.Infrastructure.ProxyProviders;

public partial class ProxyPoolService : IProxyPool
{
    private readonly ILogger<ProxyPoolService>? _logger;
    private readonly ActivitySource _activitySource = new("Agitprop.ProxyPoolService");
    private readonly IConfiguration _config;
    private readonly IProxyProvider _proxyProvider;
    private readonly HttpClient _http = new();
    private readonly ConcurrentDictionary<string, ProxyEntry> _proxies = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly PeriodicTimer _timer;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private readonly Uri _validateUri;
    private readonly int _validationTimeoutSec;
    private readonly int _validationParallelism;
    private readonly int _validationIntervalMin;
    private readonly int _maxCachedProxies;

    private readonly int _maxConnectionsPerServer;
    private readonly TimeSpan _pooledConnectionLifetime;
    private readonly TimeSpan _pooledConnectionIdleTimeout;

    public ProxyPoolService(ILogger<ProxyPoolService>? logger, IConfiguration config, IProxyProvider proxyProvider)
    {
        _logger = logger;
        _config = config;
        _proxyProvider = proxyProvider;

        _validateUri = new Uri(_config.GetValue<string>("Proxy:ValidateEndpoint", "https://vanenet.hu/"));
        _validationTimeoutSec = _config.GetValue<int>("Proxy:ValidationTimeoutSeconds", 3);
        _validationParallelism = _config.GetValue<int>("Proxy:ValidationParallelism", 50);
        _validationIntervalMin = _config.GetValue<int>("Proxy:ValidationIntervalMinutes", 3);
        _maxCachedProxies = _config.GetValue<int>("Proxy:MaxCachedProxies", 2000);

        _maxConnectionsPerServer = _config.GetValue<int>("HttpClientDefaults:MaxConnectionsPerServer", 100);
        _pooledConnectionLifetime = TimeSpan.FromSeconds(_config.GetValue<int>("HttpClientDefaults:PooledConnectionLifetimeSeconds", 30));
        _pooledConnectionIdleTimeout = TimeSpan.FromSeconds(_config.GetValue<int>("HttpClientDefaults:PooledConnectionIdleTimeoutSeconds", 30));

        _timer = new PeriodicTimer(TimeSpan.FromMinutes(_validationIntervalMin));

        this.RefreshAsync(_cts.Token).GetAwaiter().GetResult();
        // Fire-and-forget background refresh loop
        _ = BackgroundLoopAsync(_cts.Token);
    }

    private async Task BackgroundLoopAsync(CancellationToken ct)
    {
        using var activity = _activitySource.StartActivity("BackgroundLoopAsync", ActivityKind.Internal);
        activity?.SetTag("proxy.validate_interval_min", _validationIntervalMin);
        try
        {
            // initial load
            _logger?.LogInformation("Starting proxy pool background loop");
            await RefreshAsync(ct);
            while (await _timer.WaitForNextTickAsync(ct))
            {
                await RefreshAsync(ct);
            }
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("ProxyPoolService background loop cancelled");
            activity?.SetStatus(ActivityStatusCode.Ok, "Cancelled");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ProxyPoolService background loop failed");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    private async Task RefreshAsync(CancellationToken ct)
    {
        if (!await _refreshLock.WaitAsync(0, ct)) return; // skip if already running
        try
        {
            using var activity = _activitySource.StartActivity("RefreshAsync", ActivityKind.Internal);
            _logger?.LogInformation("Refreshing proxy list");

            var addresses = (await _proxyProvider.FetchProxyAddressesAsync()).Take(_maxCachedProxies).ToList();
            activity?.SetTag("proxy.fetched_addresses", addresses.Count);

            var throttler = new SemaphoreSlim(_validationParallelism);
            var tasks = new List<Task>();

            foreach (var addr in addresses)
            {
                await throttler.WaitAsync(ct);
                
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var alive = await ValidateProxyAsync(addr, ct);
                        if (alive)
                        {
                            // Only add/update if validation successful
                            _proxies.AddOrUpdate(addr,
                                // Add new
                                _ => new ProxyEntry(addr, CreateInvokerForProxy(addr)) { IsAlive = true, LastCheckedUtc = DateTime.UtcNow },
                                // Update existing
                                (_, existing) =>
                                {
                                    existing.IsAlive = true;
                                    existing.LastCheckedUtc = DateTime.UtcNow;
                                    return existing;
                                });
                        }
                        else if (_proxies.TryGetValue(addr, out var existing))
                        {
                            // Mark existing as dead if validation failed
                            existing.IsAlive = false;
                            existing.LastCheckedUtc = DateTime.UtcNow;
                            existing.DisposeInvoker();
                            _logger?.LogDebug("Proxy {Proxy} marked dead during validation", addr);
                        }
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }, ct);
                
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Remove too-old / dead entries if pool too big
            var deadKeys = _proxies.Where(kv => !kv.Value.IsAlive && (DateTime.UtcNow - kv.Value.LastCheckedUtc).TotalMinutes > 10)
                                   .Select(kv => kv.Key).ToList();

            foreach (var k in deadKeys)
            {
                if (_proxies.TryRemove(k, out var removed))
                {
                    removed.DisposeInvoker();
                    _logger?.LogDebug("Removed old dead proxy {Proxy} from cache", k);
                }
            }

            _logger?.LogInformation("Proxy refresh complete: total cached {count}, alive {aliveCount}",
                _proxies.Count, _proxies.Count(kv => kv.Value.IsAlive));
            activity?.SetTag("proxy.pool_after_refresh", _proxies.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);
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

        using var activity = _activitySource.StartActivity("CreateInvokerForProxy", ActivityKind.Internal);
        activity?.SetTag("proxy.address", addr);

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
        activity?.SetStatus(ActivityStatusCode.Ok);
        return new HttpMessageInvoker(handler, disposeHandler: true);
    }

    private async Task<bool> ValidateProxyAsync(string addr, CancellationToken ct)
    {
        // quick HEAD/GET to validation endpoint with short timeout
        using var activity = _activitySource.StartActivity("ValidateProxyAsync", ActivityKind.Internal);
        try
        {
            activity?.SetTag("proxy.address", addr);

            var invoker = _proxies.TryGetValue(addr, out var e) && e.Invoker != null
                ? e.Invoker // reuse existing invoker if present (no disposal)
                : CreateInvokerForProxy(addr);

            using var req = new HttpRequestMessage(HttpMethod.Get, _validateUri);
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; ProxyValidator/1.0)");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_validationTimeoutSec));
            var resp = await invoker.SendAsync(req, cts.Token);
            activity?.SetTag("http.status_code", (int)resp.StatusCode);
            var ok = resp.IsSuccessStatusCode;
            activity?.SetStatus(ok ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
            _logger?.LogDebug("Validated proxy {Proxy} => {Alive}", addr, ok);
            return ok;
        }
        catch(Exception ex)
        {
            _logger?.LogDebug("Validation failed for proxy {Proxy}", addr);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    public async Task<(HttpMessageInvoker? invoker, string address)?> GetRandomInvokerAsync(CancellationToken ct = default)
    {
        // Try to pick a random alive invoker
        using var activity = _activitySource.StartActivity("GetRandomInvokerAsync", ActivityKind.Internal);
        var alive = _proxies.Values.Where(e => e.IsAlive && e.Invoker != null).ToArray();
        activity?.SetTag("proxy.alive_count", alive.Length);
        if (alive.Length == 0)
        {
            // fallback: return any invoker (even if not validated) or null
            var any = _proxies.Values.Where(e => e.Invoker != null).ToArray();
            activity?.SetTag("proxy.any_count", any.Length);
            if (any.Length == 0)
            {
                _logger?.LogWarning("No proxies available in pool");
                activity?.SetStatus(ActivityStatusCode.Error, "No proxies available");
                return null;
            }
            var pick = any[new Random().Next(any.Length)];
            activity?.SetTag("proxy.selected", pick.Address);
            _logger?.LogDebug("Selected fallback proxy {Proxy}", pick.Address);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return (pick.Invoker, pick.Address);
        }

        var selected = alive[new Random().Next(alive.Length)];
        activity?.SetTag("proxy.selected", selected.Address);
        _logger?.LogDebug("Selected proxy {Proxy} from {AliveCount} alive proxies", selected.Address, alive.Length);
        activity?.SetStatus(ActivityStatusCode.Ok);
        return (selected.Invoker, selected.Address);
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
            _logger?.LogWarning("Marking proxy {Proxy} as dead", proxyAddress);
            using var activity = _activitySource.StartActivity("MarkDeadAsync", ActivityKind.Internal);
            activity?.SetTag("proxy.address", proxyAddress);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        using var activity = _activitySource.StartActivity("DisposeAsync", ActivityKind.Internal);
        _logger?.LogInformation("Disposing ProxyPoolService");
        try
        {
            _cts.Cancel();
            _timer.Dispose();
            foreach (var kv in _proxies) kv.Value.DisposeInvoker();
            _refreshLock.Dispose();
            _http.Dispose();
            _cts.Dispose();
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error while disposing ProxyPoolService");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
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
