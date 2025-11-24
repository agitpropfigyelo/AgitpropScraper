namespace Agitprop.Core.Interfaces;

using System.Net.Http;

public interface IProxyPool : IAsyncDisposable
{
    Task InitializeAsync(CancellationToken ct = default);
    Task<(HttpMessageInvoker? invoker, string address)?> GetRandomInvokerAsync(CancellationToken ct = default);
    Task<IEnumerable<string>> GetAliveProxyAddressesAsync();
    Task MarkDeadAsync(string proxyAddress);
    Task MarkSuccessAsync(string proxyAddress);
}
