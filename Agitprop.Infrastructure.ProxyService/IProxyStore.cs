namespace Agitprop.Infrastructure.ProxyService
{
    public interface IProxyStore
    {
        IReadOnlyList<ProxyInfo> GetAll();
        ProxyInfo? GetOne(Func<IEnumerable<ProxyInfo>, ProxyInfo> selector);
        Task AddOrUpdateAsync(string address, ProxyInfo info);
        Task RemoveAsync(string address);
        Task ReplaceAllAsync(IEnumerable<ProxyInfo> proxies);
    }
}