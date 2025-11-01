namespace Agitprop.Infrastructure.ProxyProviders;

public interface IProxyProvider
{
    Task<IEnumerable<string>> FetchProxyAddressesAsync();
}
