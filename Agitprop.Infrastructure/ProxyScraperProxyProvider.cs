using System.Net;

using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure;

/// <summary>
/// Provides proxies using the ProxyScrape service.
/// </summary>
public class ProxyScrapeProxyProvider : IProxyProvider
{
    // Static HTTP client for making requests to the ProxyScrape service.
    static readonly HttpClient client = new HttpClient();

    // URL to fetch the list of proxies.
    private string proxyScraperUrl = "https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/protocols/socks4/data.txt";

    // Timestamp of the last access to the proxy list.
    private DateTime lastAccessTime;

    // List of available web proxies.
    private List<WebProxy> webProxies = [];

    // Random number generator for selecting proxies.
    private readonly Random rnd = new();

    /// <summary>
    /// Gets the initialization task for the provider.
    /// </summary>
    public Task Initialization { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyScrapeProxyProvider"/> class.
    /// </summary>
    public ProxyScrapeProxyProvider()
    {
        Initialization = InitAsync();
    }

    /// <summary>
    /// Initializes the proxy list by fetching it from the ProxyScrape service.
    /// </summary>
    async Task InitAsync()
    {
        lastAccessTime = DateTime.Now;
        webProxies = (await GetWebProxies()).ToList();
    }

    /// <summary>
    /// Fetches the list of web proxies from the ProxyScrape service.
    /// </summary>
    /// <returns>An enumerable of <see cref="WebProxy"/> objects.</returns>
    private async Task<IEnumerable<WebProxy>> GetWebProxies()
    {
        string data = await (await client.GetAsync(proxyScraperUrl)).Content.ReadAsStringAsync();
        var result = data.Trim().Split('\n');
        return result.Select(proxy => new WebProxy(proxy, true));
    }

    /// <summary>
    /// Gets a random proxy from the list of available proxies.
    /// </summary>
    /// <returns>A <see cref="WebProxy"/> object.</returns>
    public async Task<WebProxy> GetProxyAsync()
    {
        if (webProxies.Count == 0) await InitAsync();
        if ((DateTime.Now - lastAccessTime).TotalMinutes > 5) await InitAsync();
        int index = rnd.Next(0, webProxies.Count);

        return webProxies[index];
    }
}
