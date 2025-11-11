namespace Agitprop.Infrastructure.ProxyService
{
    public interface IProxyValidator
    {
        Task<bool> ValidateAsync(string proxyAddress, CancellationToken ct = default);
    }
}