namespace Agitprop.Core.Interfaces;

public interface IProxyPool : IAsyncDisposable
{
    Task<string> GetNextProxyAsync(CancellationToken ct = default);
    Task MarkDeadAsync(string proxyAddress);
    Task MarkSuccessAsync(string proxyAddress);
}
