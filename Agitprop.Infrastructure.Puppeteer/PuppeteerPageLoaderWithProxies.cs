using System.Reflection;

using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.PageLoader;

using Microsoft.Extensions.Logging;

using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;

using PuppeteerSharp;

namespace Agitprop.Infrastructure.Puppeteer;

internal class PuppeteerPageLoaderWithProxies : BrowserPageLoader, IBrowserPageLoader
{
    //TODO: itt valami nem OK, debug & fix
    public PuppeteerPageLoaderWithProxies(ILogger<PuppeteerPageLoaderWithProxies> logger, IProxyProvider proxyProvider, ICookiesStorage cookieStorage) : base(logger)
    {
        ProxyProvider = proxyProvider;
        CookieStorage = cookieStorage;
    }

    public IProxyProvider ProxyProvider { get; }
    public ICookiesStorage CookieStorage { get; }
    private readonly SemaphoreSlim Semaphore = new(1, 1);
    private string? executablePath = null;

    public Task<string> Load(string url, object? pageActions, bool headless)
    {
        return Load(url, (List<PageAction>?)pageActions, headless);
    }

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
