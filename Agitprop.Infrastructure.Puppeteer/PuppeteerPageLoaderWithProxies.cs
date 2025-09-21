using Polly;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Diagnostics;

using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.PageLoader;

using Microsoft.Extensions.Logging;

using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;

using PuppeteerSharp;

namespace Agitprop.Infrastructure.Puppeteer;

/// <summary>
/// A Puppeteer-based page loader that supports proxy usage for loading web pages.
/// </summary>
internal class PuppeteerPageLoaderWithProxies : BrowserPageLoader, IBrowserPageLoader
{
    private readonly int _retryCount;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ActivitySource ActivitySource = new("Agitprop.PageLoader.PuppeteerPageLoaderWithProxies");
    private string? _executablePath;

    public PuppeteerPageLoaderWithProxies(
        ILogger<PuppeteerPageLoaderWithProxies> logger,
        IProxyProvider proxyProvider,
        ICookiesStorage cookieStorage,
        IConfiguration? configuration = null)
        : base(logger)
    {
        ProxyProvider = proxyProvider;
        CookieStorage = cookieStorage;
        _retryCount = configuration?.GetValue<int>("Retry:PuppeteerPageLoaderWithProxies", 3) ?? 3;
    }

    public IProxyProvider ProxyProvider { get; }
    public ICookiesStorage CookieStorage { get; }

    public Task<string> Load(string url, object? pageActions, bool headless)
        => Load(url, (List<PageAction>?)pageActions, headless);

    public async Task<string> Load(string url, List<PageAction>? pageActions = null, bool headless = true)
    {
        using var trace = ActivitySource.StartActivity("Load", ActivityKind.Internal);
        trace?.SetTag("url", url);
        Logger?.LogInformation("Starting page load: {url}", url);

        if (_executablePath == null)
        {
            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            });

            await _semaphore.WaitAsync();
            try
            {
                Logger?.LogInformation("Downloading browser...");
                await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(_retryCount,
                        attempt => TimeSpan.FromSeconds(0.5 * attempt),
                        (ex, ts, attempt, ctx) =>
                        {
                            Logger?.LogWarning(ex, "[RETRY] Exception downloading browser on attempt {attempt}", attempt);
                        })
                    .ExecuteAsync(() => browserFetcher.DownloadAsync());
                _executablePath = browserFetcher.GetInstalledBrowsers().First().GetExecutablePath();
                Logger?.LogInformation("Browser downloaded successfully");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        var proxy = await ProxyProvider.GetProxyAsync();
        var proxyAddress = $"--proxy-server={proxy.Address}";
        trace?.SetTag("proxy", proxy.Address);

        Logger?.LogInformation("Launching Puppeteer browser with proxy: {proxy}", proxy.Address);
        var extra = new PuppeteerExtra();
        extra.Use(new StealthPlugin());

        await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = headless,
            ExecutablePath = _executablePath,
            Args =
            [
                "--disable-infobars",
                "--ignore-certificate-errors",
                "--disable-dev-shm-usage",
                "--no-sandbox",
                "--disable-setuid-sandbox",
                proxyAddress
            ]
        });

        await using var page = (await browser.PagesAsync())[0];

        var cookies = await CookieStorage.GetAsync();
        if (cookies != null)
        {
            var cookieParams = cookies.GetAllCookies().Select(c => new CookieParam
            {
                Name = c.Name,
                Value = c.Value,
                Domain = c.Domain,
                Secure = c.Secure
            }).ToArray();

            await page.SetCookieAsync(cookieParams);
        }

        try
        {
            Logger?.LogInformation("Navigating to page: {url}", url);
            await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retryCount,
                    attempt => TimeSpan.FromSeconds(0.5 * attempt),
                    (ex, ts, attempt, ctx) =>
                    {
                        Logger?.LogWarning(ex, "[RETRY] Exception navigating to page {url} on attempt {attempt}", url, attempt);
                    })
                .ExecuteAsync(() => page.GoToAsync(url, WaitUntilNavigation.Networkidle2));
            Logger?.LogInformation("Page navigation successful: {url}", url);
        }
        catch (Exception ex)
        {
            ex.Data.Add("Proxy", proxy.Address);
            Logger?.LogError(ex, "Failed to navigate to page: {url}", url);
            trace?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        if (pageActions != null)
        {
            Logger?.LogInformation("Performing {count} page actions", pageActions.Count);
            for (int i = 0; i < pageActions.Count; i++)
            {
                var pageAction = pageActions[i];
                Logger?.LogInformation("Executing page action {current}/{total} type {actionType}",
                    i + 1, pageActions.Count, pageAction.Type);
                await PageActions[pageAction.Type](page, pageAction.Parameters);
            }
        }

        var html = await page.GetContentAsync();
        Logger?.LogInformation("Page load completed: {url}", url);
        return html;
    }
}
