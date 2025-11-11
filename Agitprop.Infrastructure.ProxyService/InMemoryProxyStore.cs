using System.Collections.Concurrent;

namespace Agitprop.Infrastructure.ProxyService
{
    public class InMemoryProxyStore : IProxyStore
    {
        // Key is address string. Use ConcurrentDictionary for lock-free reads/writes.
        private readonly ConcurrentDictionary<string, ProxyInfo> _map = new();

        public IReadOnlyList<ProxyInfo> GetAll() => _map.Values.OrderByDescending(p => p.Score).ToList();

        public ProxyInfo? GetOne(Func<IEnumerable<ProxyInfo>, ProxyInfo> selector)
        {
            var all = _map.Values.ToList();
            if (!all.Any()) return null;
            return selector(all);
        }

        public Task AddOrUpdateAsync(string address, ProxyInfo info)
        {
            _map.AddOrUpdate(address, info, (_, __) => info);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string address)
        {
            _map.TryRemove(address, out _);
            return Task.CompletedTask;
        }

        public Task ReplaceAllAsync(IEnumerable<ProxyInfo> proxies)
        {
            var newMap = new ConcurrentDictionary<string, ProxyInfo>(proxies.ToDictionary(p => p.Address));
            // Interlocked.Exchange(ref this._map, newMap); // Note: _map is readonly, so replace approach below is preferred:
            //                                         // simpler: clear and add
            _map.Clear();
            foreach (var p in proxies) _map.TryAdd(p.Address, p);
            return Task.CompletedTask;
        }
    }
}