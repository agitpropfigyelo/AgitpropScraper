using System.Diagnostics;

namespace Agitprop.Infrastructure.ProxyService
{
   public class ProxyManager : IProxyManager
{
    private readonly IProxyStore _store;
    private readonly IProxyValidator _validator;
    private readonly IEnumerable<IProxyProvider> _providers;
    private readonly SemaphoreSlim _validateThrottle;
    private readonly Random _rnd = new();
    private readonly ILogger<ProxyManager> _logger;
    private static readonly ActivitySource _activitySource = new("Agitprop.Infrastructure.ProxyService");

    public ProxyManager(
        IProxyStore store,
        IProxyValidator validator,
        IEnumerable<IProxyProvider> providers,
        ILogger<ProxyManager> logger,
        int maxConcurrentValidations = 50)
    {
        _store = store;
        _validator = validator;
        _providers = providers;
        _validateThrottle = new SemaphoreSlim(maxConcurrentValidations);
        _logger = logger;
    }

    public async Task<ProxyInfo?> GetProxyAsync(string strategy = "random")
    {
        using var activity = _activitySource.StartActivity("ProxyManager.GetProxyAsync");
        activity?.SetTag("proxy.strategy", strategy);

        var all = _store.GetAll();
        if (!all.Any())
        {
            _logger.LogWarning("No proxies available in store");
            activity?.SetStatus(ActivityStatusCode.Error, "No proxies available");
            return null;
        }

        var proxy = strategy.ToLower() switch
        {
            "roundrobin" => GetRoundRobin(all),
            "best"       => all.OrderByDescending(p => p.Score).First(),
            _            => all[_rnd.Next(all.Count)],
        };

        _logger.LogInformation("Retrieved proxy using {Strategy} strategy: {ProxyAddress}", strategy, proxy?.Address);
        activity?.SetTag("proxy.address", proxy?.Address);
        activity?.SetTag("proxy.score", proxy?.Score);

        return proxy;
    }

    private static int _rrIndex = 0;
    private ProxyInfo GetRoundRobin(IReadOnlyList<ProxyInfo> list)
    {
        var idx = Interlocked.Increment(ref _rrIndex);
        return list[idx % list.Count];
    }

    /// <summary>
    /// Fetches proxies from providers, merges them with existing ones,
    /// and validates everything. Used by background service and REST trigger.
    /// </summary>
    public async Task RefreshAllAsync(CancellationToken ct = default)
    {
        using var activity = _activitySource.StartActivity("ProxyManager.RefreshAllAsync");
        
        _logger.LogInformation("Starting proxy refresh from all providers");

        try
        {
            // 1) Fetch sources
            _logger.LogDebug("Fetching proxies from {ProviderCount} providers", _providers.Count());
            var fetchStopwatch = Stopwatch.StartNew();
            
            var fetchedLists = await Task.WhenAll(
                _providers.Select(async p =>
                {
                    using var fetchActivity = _activitySource.StartActivity("ProxyProvider.FetchProxyAddresses");
                    fetchActivity?.SetTag("provider.type", p.GetType().Name);
                    
                    try
                    {
                        var proxies = await p.FetchProxyAddressesAsync();
                        fetchActivity?.SetStatus(ActivityStatusCode.Ok);
                        fetchActivity?.SetTag("proxy.count", proxies.Count());
                        return proxies;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error fetching proxies from provider {ProviderType}", p.GetType().Name);
                        fetchActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        return Enumerable.Empty<string>();
                    }
                })
            );

            fetchStopwatch.Stop();
            _logger.LogDebug("Fetched proxy lists in {ElapsedMs}ms", fetchStopwatch.ElapsedMilliseconds);

            var fetched = fetchedLists.SelectMany(l => l).Distinct().ToList();
            var existing = _store.GetAll().Select(p => p.Address);

            _logger.LogInformation("Fetched {FetchedCount} unique proxies from providers, existing store has {ExistingCount} proxies", 
                fetched.Count, existing.Count());

            // 2) Merge
            var allToValidate = existing
                .Concat(fetched)
                .Distinct()
                .ToList();

            _logger.LogInformation("Total proxies to validate: {TotalCount}", allToValidate.Count);
            activity?.SetTag("proxy.total_to_validate", allToValidate.Count);
            activity?.SetTag("proxy.fetched_count", fetched.Count);
            activity?.SetTag("proxy.existing_count", existing.Count());

            // 3) Validate in parallel
            _logger.LogDebug("Starting parallel validation of {Count} proxies", allToValidate.Count);
            var validateStopwatch = Stopwatch.StartNew();
            
            var tasks = allToValidate.Select(addr => ValidateAndUpdateAsync(addr, ct));
            var results = await Task.WhenAll(tasks);

            validateStopwatch.Stop();
            var successCount = results.Count(r => r);
            
            _logger.LogInformation("Completed proxy validation in {ElapsedMs}ms. Valid: {ValidCount}, Invalid: {InvalidCount}", 
                validateStopwatch.ElapsedMilliseconds, successCount, results.Length - successCount);
            
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("proxy.validation_time_ms", validateStopwatch.ElapsedMilliseconds);
            activity?.SetTag("proxy.valid_count", successCount);
            activity?.SetTag("proxy.invalid_count", results.Length - successCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during proxy refresh");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<bool> ValidateAndAddAsync(string proxyAddress, CancellationToken ct = default)
    {
        using var activity = _activitySource.StartActivity("ProxyManager.ValidateAndAddAsync");
        activity?.SetTag("proxy.address", proxyAddress);
        
        _logger.LogInformation("Validating and adding proxy: {ProxyAddress}", proxyAddress);
        
        var result = await ValidateAndUpdateAsync(proxyAddress, ct);
        
        _logger.LogInformation("Validation result for {ProxyAddress}: {IsValid}", proxyAddress, result);
        activity?.SetTag("proxy.is_valid", result);
        
        return result;
    }

    private async Task<bool> ValidateAndUpdateAsync(string address, CancellationToken ct)
    {
        using var activity = _activitySource.StartActivity("ProxyManager.ValidateAndUpdateAsync");
        activity?.SetTag("proxy.address", address);

        await _validateThrottle.WaitAsync(ct);
        try
        {
            _logger.LogDebug("Starting validation for proxy: {ProxyAddress}", address);
            var validationStopwatch = Stopwatch.StartNew();

            var existing = _store.GetAll().FirstOrDefault(p => p.Address == address);
            int prevSucc = existing?.SuccessCount ?? 0;
            int prevFail = existing?.FailCount ?? 0;

            var ok = await _validator.ValidateAsync(address, ct);
            var now = DateTimeOffset.UtcNow;

            validationStopwatch.Stop();

            int succ = ok ? prevSucc + 1 : prevSucc;
            int fail = ok ? prevFail : prevFail + 1;

            var info = new ProxyInfo(address, now, succ, fail, ok);
            await _store.AddOrUpdateAsync(address, info);

            _logger.LogDebug("Validation completed for {ProxyAddress}: Valid={IsValid}, TimeMs={ElapsedMs}, SuccessCount={SuccessCount}, FailCount={FailCount}", 
                address, ok, validationStopwatch.ElapsedMilliseconds, succ, fail);

            activity?.SetTag("proxy.is_valid", ok);
            activity?.SetTag("proxy.validation_time_ms", validationStopwatch.ElapsedMilliseconds);
            activity?.SetTag("proxy.success_count", succ);
            activity?.SetTag("proxy.fail_count", fail);

            if (!ok)
            {
                _logger.LogWarning("Proxy validation failed: {ProxyAddress}", address);
            }

            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating proxy: {ProxyAddress}", address);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
        finally
        {
            _validateThrottle.Release();
        }
    }
}

}