using System.Net;
using System.Text;
using Agitprop.Core.Interfaces;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace Agitprop.Infrastructure;

public class ProxyScrapeProxyProvider : IProxyProvider
{
    static readonly HttpClient client = new HttpClient();
    private string proxyScraperUrl = "https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/protocols/socks4/data.txt";
    private DateTime lastAccessTime;

    private List<WebProxy> webProxies = new List<WebProxy>();
    private Random rnd = new Random();

    public Task Initialization { get; private set; }

    public ProxyScrapeProxyProvider()
    {
        Initialization = InitAsync();
    }

    async Task InitAsync()
    {
        lastAccessTime = DateTime.Now;
        var proxies = await GetWebProxies();
        webProxies.AddRange(proxies);
    }

    private async Task<IEnumerable<WebProxy>> GetWebProxies()
    {
        string data = await (await client.GetAsync(proxyScraperUrl)).Content.ReadAsStringAsync();
        var result = data.Trim().Split('\n');
        return result.Select(proxy => new WebProxy(proxy, true));
    }

    public async Task<IWebProxy> GetProxyAsync()
    {
        if ((DateTime.Now - lastAccessTime).TotalMinutes > 5) await InitAsync();
        int index = rnd.Next(0, webProxies.Count);

        return webProxies[index];
    }

    class FreeProxyListProxy
    {
        public string Adress { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
    }
    private class CsvProxyMapping : CsvMapping<FreeProxyListProxy>
    {
        public CsvProxyMapping()
            : base()
        {
            MapProperty(0, x => x.Adress);
            MapProperty(1, x => x.Country);
            MapProperty(2, x => x.City);
        }
    }
}
