using System.Net;
using System.Net.Security;

using Agitprop.Infrastructure.Interfaces;

namespace Agitprop.Infrastructure.PageRequester;

/// <summary>
/// A page requester that rotates proxies for each request.
/// </summary>
public class RotatingProxyPageRequester(IProxyProvider proxyProvider) : IPageRequester
{
    /// <summary>
    /// Gets the proxy provider used for rotating proxies.
    /// </summary>
    public IProxyProvider ProxyProvider { get; } = proxyProvider;

    /// <summary>
    /// Gets or sets the cookie container for managing cookies.
    /// </summary>
    public required CookieContainer CookieContainer { get; set; }

    /// <summary>
    /// Sends an HTTP GET request to the specified URL using a rotating proxy.
    /// </summary>
    /// <param name="url">The URL to send the GET request to.</param>
    /// <returns>The HTTP response message.</returns>
    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        var client = await CreateClient();
        var resp = await client.GetAsync(url);

        client.Dispose();

        return resp;
    }

    /// <summary>
    /// Creates an HTTP client with a rotating proxy and custom headers.
    /// </summary>
    /// <returns>An <see cref="HttpClient"/> instance.</returns>
    private async Task<HttpClient> CreateClient()
    {
        var handler = await GetHttpHandler();
        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/535.7 (KHTML, like Gecko) Comodo_Dragon/16.1.1.0 Chrome/16.0.912.63 Safari/535.7");
        return client;
    }

    /// <summary>
    /// Creates an HTTP handler configured with a rotating proxy and other settings.
    /// </summary>
    /// <returns>A <see cref="SocketsHttpHandler"/> instance.</returns>
    public async Task<SocketsHttpHandler> GetHttpHandler()
    {
        var handler = new SocketsHttpHandler
        {
            SslOptions = new SslClientAuthenticationOptions
            {
                // Leave certs unvalidated for debugging
                RemoteCertificateValidationCallback = delegate { return true; }
            },
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(5),
            PooledConnectionLifetime = TimeSpan.FromTicks(0),
            UseCookies = true,
            UseProxy = true,
            Proxy = await ProxyProvider.GetProxyAsync(),
            CookieContainer = CookieContainer,
            ConnectTimeout = TimeSpan.FromSeconds(60),
        };

        return handler;
    }
}
