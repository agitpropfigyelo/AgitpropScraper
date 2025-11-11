namespace Agitprop.Infrastructure;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Agitprop.Infrastructure.ProxyProviders;

public partial class ProxyPoolService : IProxyPool
{
    private readonly ILogger<ProxyPoolService>? _logger;
    private readonly ActivitySource _activitySource = new("Agitprop.ProxyPoolService");
    private readonly IConfiguration _config;
    private readonly IEnumerable<IProxyProvider> _proxyProviders;
    private readonly HttpClient _http = new();
    private readonly ConcurrentDictionary<string, ProxyEntry> _proxies = new();
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly object _roundRobinLock = new object();
    private int _roundRobinIndex = 0;

    private readonly Uri _validateUri;
    private readonly int _validationTimeoutSec;
    private readonly int _validationParallelism;
    private readonly int _minAliveProxies;
    private readonly int _targetAliveProxies;
    private readonly int _maxCachedProxies;
    private readonly TimeSpan _startupTimeout;
    private readonly TimeSpan _proxyWaitTimeout;

    private readonly int _maxConnectionsPerServer;
    private readonly TimeSpan _pooledConnectionLifetime;
    private readonly TimeSpan _pooledConnectionIdleTimeout;

    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized = false;

    public ProxyPoolService(ILogger<ProxyPoolService>? logger, IConfiguration config, IEnumerable<IProxyProvider> proxyProviders)
    {
        _logger = logger;
        _config = config;
        _proxyProviders = proxyProviders;

        // Log configuration details
        _logger?.LogInformation("Initializing ProxyPoolService with {ProviderCount} providers", _proxyProviders.Count());
        _logger?.LogDebug("Configuration - ValidateEndpoint: {Endpoint}, Timeout: {Timeout}s, Parallelism: {Parallelism}",
            _validateUri, _validationTimeoutSec, _validationParallelism);
        _logger?.LogDebug("Configuration - MinAlive: {Min}, TargetAlive: {Target}, MaxCached: {Max}",
            _minAliveProxies, _targetAliveProxies, _maxCachedProxies);
        _logger?.LogDebug("Configuration - StartupTimeout: {Startup}m, WaitTimeout: {Wait}s",
            _startupTimeout.TotalMinutes, _proxyWaitTimeout.TotalSeconds);
        _logger?.LogDebug("Configuration - HttpClient MaxConnectionsPerServer: {MaxConn}, Lifetime: {Lifetime}s, IdleTimeout: {Idle}s",
            _maxConnectionsPerServer, _pooledConnectionLifetime.TotalSeconds, _pooledConnectionIdleTimeout.TotalSeconds);

        _validateUri = new Uri(_config.GetValue<string>("Proxy:ValidateEndpoint", "https://vanenet.hu/"));
        _validationTimeoutSec = _config.GetValue<int>("Proxy:ValidationTimeoutSeconds", 3);
        _validationParallelism = _config.GetValue<int>("Proxy:ValidationParallelism", 50);
        _minAliveProxies = _config.GetValue<int>("Proxy:MinAliveProxies", 10);
        _targetAliveProxies = _config.GetValue<int>("Proxy:TargetAliveProxies", 25);
        _maxCachedProxies = _config.GetValue<int>("Proxy:MaxCachedProxies", 2000);
        _startupTimeout = TimeSpan.FromMinutes(_config.GetValue<int>("Proxy:StartupTimeoutMinutes", 5));
        _proxyWaitTimeout = TimeSpan.FromSeconds(_config.GetValue<int>("Proxy:WaitTimeoutSeconds", 30));

        _maxConnectionsPerServer = _config.GetValue<int>("HttpClientDefaults:MaxConnectionsPerServer", 100);
        _pooledConnectionLifetime = TimeSpan.FromSeconds(_config.GetValue<int>("HttpClientDefaults:PooledConnectionLifetimeSeconds", 30));
        _pooledConnectionIdleTimeout = TimeSpan.FromSeconds(_config.GetValue<int>("HttpClientDefaults:PooledConnectionIdleTimeoutSeconds", 30));
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        // Fast path: check if already initialized
        if (_isInitialized)
        {
            return;
        }

        if (!await _initializationLock.WaitAsync(0, ct))
        {
            // Another thread is already initializing, wait for it to complete
            _logger?.LogDebug("Waiting for another thread to complete proxy pool initialization");
            var waitEndTime = DateTime.UtcNow.Add(_startupTimeout);
            while (!_isInitialized && DateTime.UtcNow < waitEndTime)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), ct);
            }
            if (_isInitialized)
            {
                _logger?.LogInformation("Proxy pool initialization completed by another thread");
            }
            else
            {
                _logger?.LogWarning("Timeout waiting for proxy pool initialization by another thread");
            }
            return;
        }

        try
        {
            using var activity = _activitySource.StartActivity("InitializeAsync", ActivityKind.Internal);
            _logger?.LogInformation("Initializing proxy pool with target of {TargetAlive} alive proxies (timeout: {Timeout}m)",
                _targetAliveProxies, _startupTimeout.TotalMinutes);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_startupTimeout);

            var startTime = DateTime.UtcNow;
            var lastAliveCount = 0;
            var noProgressCount = 0;

            while (!cts.Token.IsCancellationRequested)
            {
                int aliveCount;
                try
                {
                    aliveCount = await RefreshWithProgressTrackingAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    // Timeout reached, exit loop gracefully
                    break;
                }

                if (aliveCount >= _targetAliveProxies)
                {
                    _logger?.LogInformation("Proxy pool initialized successfully with {AliveCount} alive proxies (target: {TargetAlive})",
                        aliveCount, _targetAliveProxies);
                    _isInitialized = true;
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return;
                }

                // Check if we're making progress
                if (aliveCount == lastAliveCount)
                {
                    noProgressCount++;
                    if (noProgressCount >= 3)
                    {
                        _logger?.LogWarning("No progress in proxy validation after {Elapsed}m, proceeding with {AliveCount} proxies (target: {TargetAlive})",
                            (DateTime.UtcNow - startTime).TotalMinutes, aliveCount, _targetAliveProxies);
                        _isInitialized = true;
                        break;
                    }
                }
                else
                {
                    noProgressCount = 0;
                    lastAliveCount = aliveCount;
                }

                _logger?.LogInformation("Proxy pool initialization: {AliveCount}/{TargetAlive} alive proxies after {Elapsed}m, continuing validation",
                    aliveCount, _targetAliveProxies, (DateTime.UtcNow - startTime).TotalMinutes);

                // Wait before next validation attempt
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                }
                catch (TaskCanceledException)
                {
                    // Timeout reached, exit loop gracefully
                    break;
                }
            }

            if (cts.Token.IsCancellationRequested)
            {
                var finalCount = _proxies.Count(kv => kv.Value.IsAlive);
                _logger?.LogWarning("Proxy pool initialization timed out after {Timeout}m with {AliveCount} proxies (target: {TargetAlive})",
                    _startupTimeout.TotalMinutes, finalCount, _targetAliveProxies);
            }

            _isInitialized = true;
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task<int> RefreshWithProgressTrackingAsync(CancellationToken ct)
    {
        _logger?.LogTrace("Starting refresh with progress tracking");
        await RefreshAsync(ct);
        var aliveCount = _proxies.Count(kv => kv.Value.IsAlive && kv.Value.Invoker != null);
        _logger?.LogTrace("Refresh with progress tracking completed. Alive proxies: {AliveCount}", aliveCount);
        return aliveCount;
    }



    private async Task RefreshAsync(CancellationToken ct)
    {
        using var activity = _activitySource.StartActivity("RefreshAsync", ActivityKind.Internal);
        if (!await _refreshLock.WaitAsync(0, ct))
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Another refresh already in progress");
            _logger?.LogDebug("Another proxy refresh already in progress, skipping this request");
            return; // skip if already running
        }
        try
        {
            _logger?.LogInformation("Refreshing proxy list until {TargetAlive} alive proxies", _targetAliveProxies);

            var addresses = new List<string>();
            var providerResults = new List<string>();
            
            _logger?.LogDebug("Fetching proxy addresses from {ProviderCount} providers", _proxyProviders.Count());
            foreach (var provider in _proxyProviders)
            {
                try
                {
                    _logger?.LogTrace("Fetching proxies from provider: {Provider}", provider.GetType().Name);
                    var providerAddresses = (await provider.FetchProxyAddressesAsync()).Take(_maxCachedProxies).ToList();
                    addresses.AddRange(providerAddresses);
                    providerResults.Add($"{provider.GetType().Name}:{providerAddresses.Count}");
                    _logger?.LogInformation("Fetched {count} proxy addresses from provider {provider}", providerAddresses.Count, provider.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to fetch proxy addresses from provider {provider}", provider.GetType().Name);
                    providerResults.Add($"{provider.GetType().Name}:ERROR");
                }
            }
            
            _logger?.LogDebug("Provider fetch results: {Results}", string.Join(", ", providerResults));
            activity?.SetTag("proxy.fetched_addresses", addresses.Count);

            // Shuffle addresses to avoid checking in the same order
            var shuffledAddresses = addresses.OrderBy(a => Guid.NewGuid()).ToList();
            _logger?.LogDebug("Shuffled {Count} proxy addresses for validation", shuffledAddresses.Count);
            _logger?.LogTrace("First 5 addresses to validate: {Addresses}", string.Join(", ", shuffledAddresses.Take(5)));

            var throttler = new SemaphoreSlim(_validationParallelism);
            var tasks = new List<Task>();
            var aliveProxiesFound = 0;
            var validationStopwatch = Stopwatch.StartNew();
            
            _logger?.LogDebug("Starting proxy validation with {Parallelism} parallel workers", _validationParallelism);

            var totalValidated = 0;
            var totalFailed = 0;
            
            foreach (var addr in shuffledAddresses)
            {
                // Stop if we've reached our target
                if (aliveProxiesFound >= _targetAliveProxies)
                {
                    _logger?.LogInformation("Reached target of {TargetAlive} alive proxies, stopping validation", _targetAliveProxies);
                    break;
                }
                
                totalValidated++;

                await throttler.WaitAsync(ct);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        var alive = await ValidateProxyAsync(addr, ct);
                        if (alive)
                        {
                            Interlocked.Increment(ref aliveProxiesFound);
                            Interlocked.Increment(ref totalValidated);
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
                            _logger?.LogDebug("Proxy {Proxy} validated as alive ({AliveCount}/{TargetAlive}/{TotalValidated})", addr, aliveProxiesFound, _targetAliveProxies, totalValidated);
                        }
                        else
                        {
                            Interlocked.Increment(ref totalFailed);
                            if (_proxies.TryGetValue(addr, out var existing))
                            {
                                // Mark existing as dead if validation failed
                                existing.IsAlive = false;
                                existing.LastCheckedUtc = DateTime.UtcNow;
                                existing.DisposeInvoker();
                                _logger?.LogTrace("Proxy {Proxy} marked dead during validation", addr);
                            }
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
            validationStopwatch.Stop();
            
            _logger?.LogDebug("Proxy validation completed. Total: {Total}, Alive: {Alive}, Failed: {Failed}, Duration: {Duration}ms",
                totalValidated + totalFailed, aliveProxiesFound, totalFailed, validationStopwatch.ElapsedMilliseconds);

            // Remove too-old / dead entries if pool too big
            var deadKeys = _proxies.Where(kv => !kv.Value.IsAlive && (DateTime.UtcNow - kv.Value.LastCheckedUtc).TotalMinutes > 10)
                                   .Select(kv => kv.Key).ToList();

            _logger?.LogTrace("Checking for old dead proxies to remove. Current dead count: {DeadCount}", deadKeys.Count);
            foreach (var k in deadKeys)
            {
                if (_proxies.TryRemove(k, out var removed))
                {
                    removed.DisposeInvoker();
                    _logger?.LogTrace("Removed old dead proxy {Proxy} from cache", k);
                }
            }
            
            if (deadKeys.Count > 0)
            {
                _logger?.LogDebug("Removed {Count} old dead proxies from cache", deadKeys.Count);
            }

            var finalAliveCount = _proxies.Count(kv => kv.Value.IsAlive);
            _logger?.LogInformation("Proxy refresh complete in {ElapsedMs}ms: total cached {count}, alive {aliveCount} (target: {TargetAlive})",
                validationStopwatch.ElapsedMilliseconds, _proxies.Count, finalAliveCount, _targetAliveProxies);
            activity?.SetTag("proxy.pool_after_refresh", _proxies.Count);
            activity?.SetTag("proxy.alive_after_refresh", finalAliveCount);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private HttpMessageInvoker CreateInvokerForProxy(string addr)
    {
        _logger?.LogTrace("Creating HTTP invoker for proxy: {Address}", addr);
        // addr = "ip:port"
        var parts = addr.Split(':');
        var host = parts[0];
        var port = int.Parse(parts[1]);
        
        _logger?.LogTrace("Parsed proxy - Host: {Host}, Port: {Port}", host, port);

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
        
        _logger?.LogTrace("Created SocketsHttpHandler for proxy {Address} with connect timeout: {Timeout}s", addr, _validationTimeoutSec);

        // Create invoker (wrapper over handler) so we can reuse it and dispose handler later
        activity?.SetStatus(ActivityStatusCode.Ok);
        var invoker = new HttpMessageInvoker(handler, disposeHandler: true);
        _logger?.LogTrace("Created HttpMessageInvoker for proxy {Address}", addr);
        return invoker;
    }

    private async Task<bool> ValidateProxyAsync(string addr, CancellationToken ct)
    {
        // quick HEAD/GET to validation endpoint with short timeout
        using var activity = _activitySource.StartActivity("ValidateProxyAsync", ActivityKind.Internal);
        
        _logger?.LogTrace("Starting validation for proxy: {Address}", addr);
        
        try
        {
            activity?.SetTag("proxy.address", addr);
            activity?.SetTag("validation.endpoint", _validateUri.ToString());
            activity?.SetTag("validation.timeout_sec", _validationTimeoutSec);

            HttpMessageInvoker invoker;
            if (_proxies.TryGetValue(addr, out var e) && e.Invoker != null)
            {
                invoker = e.Invoker; // reuse existing invoker if present (no disposal)
                _logger?.LogTrace("Reusing existing invoker for proxy {Proxy}", addr);
            }
            else
            {
                invoker = CreateInvokerForProxy(addr);
                if (invoker == null)
                {
                    _logger?.LogTrace("Failed to create invoker for proxy {Proxy}", addr);
                    activity?.SetStatus(ActivityStatusCode.Error, "Failed to create invoker");
                    return false;
                }
                _logger?.LogTrace("Created new invoker for proxy {Proxy}", addr);
            }

            using var req = new HttpRequestMessage(HttpMethod.Get, _validateUri);
            req.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; ProxyValidator/1.0)");
            
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_validationTimeoutSec));
            
            _logger?.LogTrace("Sending validation request for proxy {Proxy} to {Endpoint}", addr, _validateUri);
            
            var resp = await invoker.SendAsync(req, cts.Token);
            activity?.SetTag("http.status_code", (int)resp.StatusCode);
            var ok = resp.IsSuccessStatusCode;
            
            if (ok)
            {
                _logger?.LogTrace("Proxy {Proxy} validation successful - Status: {Status}", addr, resp.StatusCode);
            }
            else
            {
                _logger?.LogTrace("Proxy {Proxy} validation failed - Status: {Status}", addr, resp.StatusCode);
            }
            
            activity?.SetStatus(ok ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
            return ok;
        }
        catch (OperationCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger?.LogTrace("Proxy {Proxy} validation timed out after {Timeout}s", addr, _validationTimeoutSec);
            activity?.SetStatus(ActivityStatusCode.Error, "Validation timeout");
            return false;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogTrace("Proxy {Proxy} validation was cancelled", addr);
            activity?.SetStatus(ActivityStatusCode.Error, "Validation cancelled");
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogTrace(ex, "Validation failed for proxy {Proxy} - Exception: {ExceptionType}", addr, ex.GetType().Name);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    public async Task<(HttpMessageInvoker? invoker, string address)?> GetRandomInvokerAsync(CancellationToken ct = default)
    {
        // Try to pick a round-robin alive invoker
        using var activity = _activitySource.StartActivity("GetRandomInvokerAsync", ActivityKind.Internal);
        
        _logger?.LogTrace("GetRandomInvokerAsync called. Current pool state - Total: {Total}, Alive: {Alive}", 
            _proxies.Count, _proxies.Count(kv => kv.Value.IsAlive));

        var attemptCount = 0;
        var maxAttempts = 3;

        while (attemptCount < maxAttempts)
        {
            attemptCount++;

            // Wait for initialization to complete
            if (!_isInitialized)
            {
                _logger?.LogWarning("Proxy pool not yet initialized, waiting for initialization to complete (attempt {Attempt}/{MaxAttempts})",
                    attemptCount, maxAttempts);
                try
                {
                    // Simply wait for initialization to complete, don't touch the lock
                    await Task.Delay(_proxyWaitTimeout, ct);
                }
                catch (TimeoutException)
                {
                    _logger?.LogWarning("Timeout waiting for proxy pool initialization (attempt {Attempt}/{MaxAttempts})",
                        attemptCount, maxAttempts);
                    if (attemptCount >= maxAttempts)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, "Initialization timeout");
                        throw new InvalidOperationException($"Proxy pool initialization timeout after {maxAttempts} attempts");
                    }
                }
            }

            // Check if we need to refresh the proxy pool
            var currentAliveCount = _proxies.Count(kv => kv.Value.IsAlive && kv.Value.Invoker != null);
            var currentTotalCount = _proxies.Count;
            
            _logger?.LogDebug("Proxy pool status - Total: {Total}, Alive: {Alive}, MinRequired: {MinAlive}", 
                currentTotalCount, currentAliveCount, _minAliveProxies);
                
            if (currentAliveCount < _minAliveProxies)
            {
                _logger?.LogWarning("Only {AliveCount} alive proxies available (minimum: {MinAlive}), starting background refresh", currentAliveCount, _minAliveProxies);
                // Start refresh in background without waiting
                _ = Task.Run(async () => await RefreshAsync(ct), ct);
            }

            var alive = _proxies.Values.Where(e => e.IsAlive && e.Invoker != null).ToArray();
            activity?.SetTag("proxy.alive_count", alive.Length);
            
            _logger?.LogTrace("Found {AliveCount} alive proxies for selection", alive.Length);
            
            if (alive.Length > 0)
            {
                var addresses = alive.Select(e => e.Address).Take(3).ToArray();
                _logger?.LogTrace("First few alive proxies: {Addresses}", string.Join(", ", addresses));
            }

            if (alive.Length == 0)
            {
                if (!_isInitialized)
                {
                    // Still initializing, wait and retry
                    _logger?.LogWarning("No proxies available and pool not initialized, waiting and retrying (attempt {Attempt}/{MaxAttempts})",
                        attemptCount, maxAttempts);
                    await Task.Delay(_proxyWaitTimeout, ct);
                    continue;
                }

                // Pool is initialized but no proxies available - block and wait for refresh
                _logger?.LogWarning("No proxies available in pool, blocking and waiting for refresh (attempt {Attempt}/{MaxAttempts})",
                    attemptCount, maxAttempts);

                // Start a refresh
                _ = Task.Run(async () => await RefreshAsync(ct), ct);

                // Wait for proxies to become available
                await Task.Delay(_proxyWaitTimeout, ct);

                if (attemptCount < maxAttempts)
                {
                    continue;
                }

                // Last attempt - try to get any proxy (even if not validated)
                var any = _proxies.Values.Where(e => e.Invoker != null).ToArray();
                _logger?.LogWarning("Attempting fallback proxy selection. Found {AnyCount} total proxies with invokers", any.Length);
                if (any.Length > 0)
                {
                    ProxyEntry selected;
                    lock (_roundRobinLock)
                    {
                        if (_roundRobinIndex >= any.Length) _roundRobinIndex = 0;
                        selected = any[_roundRobinIndex++];
                    }
                    activity?.SetTag("proxy.selected", selected.Address);
                    _logger?.LogWarning("Selected fallback proxy {Proxy} on attempt {Attempt}. Proxy status: {IsAlive}", 
                        selected.Address, attemptCount, selected.IsAlive);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return (selected.Invoker, selected.Address);
                }

                var finalAliveCount = _proxies.Count(kv => kv.Value.IsAlive);
                var finalTotalCount = _proxies.Count;
                _logger?.LogError("No proxies available in pool after {MaxAttempts} attempts. Final state - Total: {Total}, Alive: {Alive}", 
                    maxAttempts, finalTotalCount, finalAliveCount);
                activity?.SetStatus(ActivityStatusCode.Error, "No proxies available");
                throw new InvalidOperationException($"No proxies available in pool after {maxAttempts} attempts");
            }

            // We have alive proxies
            ProxyEntry selectedEntry;
            lock (_roundRobinLock)
            {
                if (_roundRobinIndex >= alive.Length) _roundRobinIndex = 0;
                selectedEntry = alive[_roundRobinIndex++];
                _logger?.LogTrace("Round-robin selection: index {Index} from {AliveCount} options", _roundRobinIndex - 1, alive.Length);
            }

            activity?.SetTag("proxy.selected", selectedEntry.Address);
            _logger?.LogDebug("Selected proxy {Proxy} from {AliveCount} alive proxies (round-robin index: {Index})",
                selectedEntry.Address, alive.Length, _roundRobinIndex - 1);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return (selectedEntry.Invoker, selectedEntry.Address);
        }

        // Should not reach here
        activity?.SetStatus(ActivityStatusCode.Error, "Failed to get proxy after retries");
        return null;
    }

    public Task<IEnumerable<string>> GetAliveProxyAddressesAsync()
    {
        var aliveProxies = _proxies.Where(kv => kv.Value.IsAlive).Select(kv => kv.Key).AsEnumerable();
        var count = aliveProxies.Count();
        _logger?.LogDebug("GetAliveProxyAddressesAsync called. Returning {Count} alive proxy addresses", count);
        _logger?.LogTrace("Alive proxy addresses: {Addresses}", string.Join(", ", aliveProxies.Take(10)));
        return Task.FromResult(aliveProxies);
    }

    public Task MarkDeadAsync(string proxyAddress)
    {
        _logger?.LogTrace("MarkDeadAsync called for proxy: {Proxy}", proxyAddress);
        
        if (_proxies.TryGetValue(proxyAddress, out var entry))
        {
            _logger?.LogWarning("Marking proxy {Proxy} as dead. Current state: IsAlive={IsAlive}, LastChecked={LastChecked}", 
                proxyAddress, entry.IsAlive, entry.LastCheckedUtc);
                
            entry.IsAlive = false;
            entry.LastCheckedUtc = DateTime.UtcNow;
            entry.DisposeInvoker();
            
            var remainingAlive = _proxies.Count(kv => kv.Value.IsAlive);
            _logger?.LogInformation("Proxy {Proxy} marked as dead. Remaining alive proxies: {AliveCount}", proxyAddress, remainingAlive);
            
            using var activity = _activitySource.StartActivity("MarkDeadAsync", ActivityKind.Internal);
            activity?.SetTag("proxy.address", proxyAddress);
            activity?.SetTag("proxy.remaining_alive", remainingAlive);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        else
        {
            _logger?.LogWarning("Attempted to mark proxy {Proxy} as dead, but proxy not found in pool", proxyAddress);
        }
        
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        using var activity = _activitySource.StartActivity("DisposeAsync", ActivityKind.Internal);
        
        var proxyCount = _proxies.Count;
        var aliveCount = _proxies.Count(kv => kv.Value.IsAlive);
        _logger?.LogInformation("Disposing ProxyPoolService - Total proxies: {Total}, Alive: {Alive}", proxyCount, aliveCount);
        
        try
        {
            var disposedInvokers = 0;
            foreach (var kv in _proxies) 
            {
                if (kv.Value.Invoker != null)
                {
                    disposedInvokers++;
                }
                kv.Value.DisposeInvoker();
            }
            
            _logger?.LogDebug("Disposed {Count} HTTP invokers from proxy pool", disposedInvokers);
            
            _refreshLock.Dispose();
            _http.Dispose();
            _logger?.LogDebug("Disposed refresh lock and HTTP client");
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger?.LogInformation("ProxyPoolService disposed successfully");
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
