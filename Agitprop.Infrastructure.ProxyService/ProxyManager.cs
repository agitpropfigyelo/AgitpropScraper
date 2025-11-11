namespace Agitprop.Infrastructure.ProxyService
{
   public class ProxyManager : IProxyManager
{
    private readonly IProxyStore _store;
    private readonly IProxyValidator _validator;
    private readonly IEnumerable<IProxyProvider> _providers;
    private readonly SemaphoreSlim _validateThrottle;
    private readonly Random _rnd = new();

    public ProxyManager(
        IProxyStore store,
        IProxyValidator validator,
        IEnumerable<IProxyProvider> providers,
        int maxConcurrentValidations = 50)
    {
        _store = store;
        _validator = validator;
        _providers = providers;
        _validateThrottle = new SemaphoreSlim(maxConcurrentValidations);
    }

    public async Task<ProxyInfo?> GetProxyAsync(string strategy = "random")
    {
        var all = _store.GetAll();
        if (!all.Any()) return null;

        return strategy.ToLower() switch
        {
            "roundrobin" => GetRoundRobin(all),
            "best"       => all.OrderByDescending(p => p.Score).First(),
            _            => all[_rnd.Next(all.Count)],
        };
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
        // 1) Fetch sources
        var fetchedLists = await Task.WhenAll(
            _providers.Select(p => p.FetchProxyAddressesAsync())
        );

        var fetched = fetchedLists.SelectMany(l => l).Distinct().ToList();
        var existing = _store.GetAll().Select(p => p.Address);

        // 2) Merge
        var allToValidate = existing
            .Concat(fetched)
            .Distinct()
            .ToList();

        // 3) Validate in parallel
        var tasks = allToValidate.Select(addr => ValidateAndUpdateAsync(addr, ct));
        await Task.WhenAll(tasks);
    }

    public async Task<bool> ValidateAndAddAsync(string proxyAddress, CancellationToken ct = default)
        => await ValidateAndUpdateAsync(proxyAddress, ct);

    private async Task<bool> ValidateAndUpdateAsync(string address, CancellationToken ct)
    {
        await _validateThrottle.WaitAsync(ct);
        try
        {
            var ok = await _validator.ValidateAsync(address, ct);
            var now = DateTimeOffset.UtcNow;

            var existing = _store.GetAll().FirstOrDefault(p => p.Address == address);
            int succ = existing?.SuccessCount ?? 0;
            int fail = existing?.FailCount ?? 0;

            if (ok) succ++; else fail++;

            var info = new ProxyInfo(address, now, succ, fail, ok);
            await _store.AddOrUpdateAsync(address, info);

            return ok;
        }
        finally
        {
            _validateThrottle.Release();
        }
    }
}

}