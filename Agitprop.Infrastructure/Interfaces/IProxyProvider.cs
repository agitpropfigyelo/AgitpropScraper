using System.Net;

namespace Agitprop.Infrastructure.Interfaces;

/// <summary>
/// Defines the contract for providing web proxies.
/// </summary>
public interface IProxyProvider
{
    /// <summary>
    /// Retrieves a web proxy for use in HTTP requests.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="WebProxy"/> instance.</returns>
    Task<WebProxy> GetProxyAsync();
}
