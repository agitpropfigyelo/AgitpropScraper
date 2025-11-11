namespace Agitprop.Infrastructure.ProxyService
{
    public interface IProxyProvider
    {
        Task<IEnumerable<string>> FetchProxyAddressesAsync();
    }
}