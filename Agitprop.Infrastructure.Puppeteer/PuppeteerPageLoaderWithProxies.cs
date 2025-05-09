using System.Reflection;

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
    /// <summary>
    /// Initializes a new instance of the <see cref="PuppeteerPageLoaderWithProxies"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging information and errors.</param>
    /// <param name="proxyProvider">The provider for managing proxies.</param>
    /// <param name="cookieStorage">The storage for managing cookies.</param>
    public PuppeteerPageLoaderWithProxies(ILogger<PuppeteerPageLoaderWithProxies> logger, IProxyProvider proxyProvider, ICookiesStorage cookieStorage) : base(logger)
    {
        ProxyProvider = proxyProvider;
        CookieStorage = cookieStorage;
    }

    /// <summary>
    /// Gets the proxy provider used for managing proxies.
    /// </summary>
    public IProxyProvider ProxyProvider { get; }

    /// <summary>
    /// Gets the storage for managing cookies.
    /// </summary>
    public ICookiesStorage CookieStorage { get; }
    private readonly SemaphoreSlim Semaphore = new(1, 1);
    private string? executablePath = null;

    /// <summary>
    /// Loads a web page using Puppeteer with proxy support, with optional page actions and headless mode.
    /// </summary>
    /// <param name="url">The URL of the page to load.</param>
    /// <param name="pageActions">Optional actions to perform on the page, as an object.</param>
    /// <param name="headless">Indicates whether the browser should run in headless mode.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the page content as a string.</returns>
    public Task<string> Load(string url, object? pageActions, bool headless)
    {
        return Load(url, (List<PageAction>?)pageActions, headless);
    }

    /// <summary>
    /// Loads a web page using Puppeteer with proxy support, with optional page actions and headless mode.
    /// </summary>
    /// <param name="url">The URL of the page to load.</param>
    /// <param name="pageActions">Optional actions to perform on the page, as a list of <see cref="PageAction"/>.</param>
    /// <param name="headless">Indicates whether the browser should run in headless mode.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the page content as a string.</returns>
    public async Task<string> Load(string url, List<PageAction>? pageActions = null, bool headless = true)
    {
        if (executablePath == null)
        {

            var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
            {
                Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            });

            await Semaphore.WaitAsync();
            try
            {
                Logger?.LogInformation("{class}.{method}: Downloading browser...", nameof(PuppeteerPageLoaderWithProxies), nameof(Load));
                await browserFetcher.DownloadAsync();
                executablePath = browserFetcher.GetInstalledBrowsers().First().GetExecutablePath();
                Logger?.LogInformation("{class}.{method}: Browser is downloaded", nameof(PuppeteerPageLoaderWithProxies), nameof(Load));
            }
            finally
            {
                Semaphore.Release();
            }
        }

        //var puppeteerExtra = new PuppeteerExtra().Use(new StealthPlugin());

        var proxy = await ProxyProvider.GetProxyAsync();
        //var proxyAddress = $"--proxy-server={proxy.Address!.Host}:{proxy.Address.Port}";
        var proxyAddress = $"--proxy-server={proxy.Address}";

        var extra = new PuppeteerExtra();

        // Use stealth plugin
        extra.Use(new StealthPlugin());
        //extra.Use(new AnonymizeUaPlugin());

        await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = headless,
            ExecutablePath = executablePath,
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

            await page.GoToAsync(url, WaitUntilNavigation.Networkidle2);
        }
        catch (Exception ex)
        {
            ex.Data.Add("Proxy:", proxy.Address);
            Logger?.LogError(ex, "Failed to open page: {url}",url);
            throw;
        }

        if (pageActions != null)
        {
            Logger?.LogInformation("{class}.{method}: performing page actions", nameof(PuppeteerPageLoaderWithProxies), nameof(Load));

            for (int i = 0; i < pageActions.Count; i++)
            {
                var pageAction = pageActions[i];
                Logger?.LogInformation("{class}.{method}: performing page action {current} of {count} with type {actionType}",
                    nameof(PuppeteerPageLoaderWithProxies),
                    nameof(Load),
                    i,
                    pageActions.Count - 1,
                    pageAction.Type);

                await PageActions[pageAction.Type](page, pageAction.Parameters);
            }
        }

        var html = await page.GetContentAsync();

        return html;
    }
}
