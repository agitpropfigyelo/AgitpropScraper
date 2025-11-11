namespace Agitprop.Infrastructure.ProxyService
{
    public interface IProxyManager
    {
        Task<ProxyInfo?> GetProxyAsync(string strategy = "random");
        Task RefreshAllAsync(CancellationToken ct = default);     // re-validate existing list
        Task<bool> ValidateAndAddAsync(string proxyAddress, CancellationToken ct = default);
    }
}