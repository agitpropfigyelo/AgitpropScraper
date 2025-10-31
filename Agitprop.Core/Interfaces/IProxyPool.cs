namespace Agitprop.Core.Interfaces;

using System.Net.Http;

public interface IProxyPool : IAsyncDisposable
{
    Task<(HttpMessageInvoker? invoker, string address)?> GetRandomInvokerAsync(CancellationToken ct = default);
    Task<IEnumerable<string>> GetAliveProxyAddressesAsync();
    Task MarkDeadAsync(string proxyAddress);
}
