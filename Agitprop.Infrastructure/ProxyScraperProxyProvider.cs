using System.Net;

using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure;

public class ProxyScrapeProxyProvider : IProxyProvider
{
    static readonly HttpClient client = new HttpClient();
    private string proxyScraperUrl = "https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/protocols/socks4/data.txt";
    //TODO: request the list after every 5 min improve thread safety
    private DateTime lastAccessTime;
    private List<WebProxy> webProxies = [];
    private readonly Random rnd = new();

    public Task Initialization { get; private set; }

    public ProxyScrapeProxyProvider()
    {
        Initialization = InitAsync();
    }

    async Task InitAsync()
    {
        lastAccessTime = DateTime.Now;
        webProxies = (await GetWebProxies()).ToList();
    }

    private async Task<IEnumerable<WebProxy>> GetWebProxies()
    {
        string data = await (await client.GetAsync(proxyScraperUrl)).Content.ReadAsStringAsync();
        var result = data.Trim().Split('\n');
        return result.Select(proxy => new WebProxy(proxy, true));
    }

    public async Task<WebProxy> GetProxyAsync()
    {
        if (webProxies.Count == 0) await InitAsync();
        if ((DateTime.Now - lastAccessTime).TotalMinutes > 5) await InitAsync();
        int index = rnd.Next(0, webProxies.Count);

        return webProxies[index];
    }
}
