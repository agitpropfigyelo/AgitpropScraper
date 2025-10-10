using Polly;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Diagnostics;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure;

public class ProxyScrapeProxyProvider : IProxyProvider
{
    private static readonly HttpClient _client = new();
    private readonly ILogger<ProxyScrapeProxyProvider>? _logger;
    private readonly int _retryCount;
    private readonly Random _rnd = new();
    private readonly ActivitySource _activitySource = new("Agitprop.ProxyScrapeProxyProvider");

    private string _proxyScraperUrl = "https://raw.githubusercontent.com/proxifly/free-proxy-list/main/proxies/protocols/socks4/data.txt";
    private DateTime _lastAccessTime;
    private List<WebProxy> _webProxies = new();

    public Task Initialization { get; private set; }

    public ProxyScrapeProxyProvider(ILogger<ProxyScrapeProxyProvider>? logger = null, IConfiguration? configuration = null)
    {
        _logger = logger;
        _retryCount = configuration?.GetValue<int>("Retry:ProxyProvider", 3) ?? 3;
        Initialization = InitAsync();
    }

    private async Task InitAsync()
    {
        using var activity = _activitySource.StartActivity("InitAsync");
        activity?.SetTag("proxyScraperUrl", _proxyScraperUrl);

        _logger?.LogInformation("Initializing ProxyScrape provider...");
        _lastAccessTime = DateTime.Now;
        _webProxies = (await GetWebProxiesAsync()).ToList();

        _logger?.LogInformation("Initialized {count} proxies at {time}", _webProxies.Count, _lastAccessTime);
    }

    private async Task<IEnumerable<WebProxy>> GetWebProxiesAsync()
    {
        using var activity = _activitySource.StartActivity("GetWebProxies");
        activity?.SetTag("proxyScraperUrl", _proxyScraperUrl);

        string data = string.Empty;

        try
        {
            var response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt),
                    (outcome, ts, attempt, ctx) =>
                    {
                        if (outcome.Exception != null)
                            _logger?.LogWarning(outcome.Exception, "[RETRY] Exception fetching proxies from {url} on attempt {attempt}", _proxyScraperUrl, attempt);
                        else if (outcome.Result != null)
                            _logger?.LogWarning("[RETRY] Failed to fetch proxies from {url} on attempt {attempt}. Status: {statusCode}", _proxyScraperUrl, attempt, outcome.Result.StatusCode);
                    })
                .ExecuteAsync(() => _client.GetAsync(_proxyScraperUrl));

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("Failed to fetch proxies from {url}. Status: {statusCode}", _proxyScraperUrl, response.StatusCode);
                throw new InvalidOperationException($"Failed to fetch proxies from {_proxyScraperUrl}. Status: {response.StatusCode}");
            }

            data = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception thrown while fetching proxies from {url}", _proxyScraperUrl);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        var proxies = data.Trim().Split('\n').Select(p => new WebProxy(p, true)).ToList();
        activity?.SetTag("proxyCount", proxies.Count);
        _logger?.LogInformation("Fetched {count} proxies successfully", proxies.Count);

        return proxies;
    }

    public async Task<WebProxy> GetProxyAsync()
    {
        using var activity = _activitySource.StartActivity("GetProxy");
        activity?.SetTag("proxyCountBefore", _webProxies.Count);

        if (!_webProxies.Any() || (DateTime.Now - _lastAccessTime).TotalMinutes > 5)
        {
            _logger?.LogInformation("Refreshing proxy list (last access: {time})", _lastAccessTime);
            await InitAsync();
        }

        int index = _rnd.Next(0, _webProxies.Count);
        var selectedProxy = _webProxies[index];

        _logger?.LogInformation("Returning proxy {proxy} (index {index})", selectedProxy.Address, index);
        activity?.SetTag("proxyIndex", index);

        return selectedProxy;
    }
}
