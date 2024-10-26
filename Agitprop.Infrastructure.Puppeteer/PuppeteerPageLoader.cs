using System.Reflection;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.PageLoader;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace Agitprop.Infrastructure.Puppeteer;

public class PuppeteerPageLoader : BrowserPageLoader, IBrowserPageLoader
{
    private readonly ICookiesStorage _cookiesStorage;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public PuppeteerPageLoader(ILogger<PuppeteerPageLoader> logger, ICookiesStorage cookiesStorage) : base(logger)
    {
        _cookiesStorage = cookiesStorage;
    }

    public async Task<string> Load(string url, List<PageAction>? pageActions = null, bool headless = true)
    {
        Logger.LogInformation("{class}.{method}", nameof(PuppeteerPageLoader), nameof(Load));

        using var _ = Logger.LogMethodDuration();

        var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
        {
            Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        });

        await _semaphore.WaitAsync();
        try
        {
            Logger.LogInformation("{class}.{method}: Downloading browser...", nameof(PuppeteerPageLoader), nameof(Load));
            await browserFetcher.DownloadAsync(BrowserTag.Stable);
            Logger.LogInformation("{class}.{method}: Browser is downloaded", nameof(PuppeteerPageLoader), nameof(Load));
        }
        finally
        {
            _semaphore.Release();
        }
        PuppeteerSharp.BrowserData.InstalledBrowser idk = browserFetcher.GetInstalledBrowsers().First();
        Logger.LogInformation("{class}.{method}: Launching a browser", nameof(PuppeteerPageLoader), nameof(Load));
        await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = headless,
            ExecutablePath = browserFetcher.GetInstalledBrowsers().First().GetExecutablePath(),
        });

        Logger.LogInformation("{class}.{method}: creating a new page", nameof(PuppeteerPageLoader), nameof(Load));
        await using var page = (await browser.PagesAsync())[0];

        var cookies = await _cookiesStorage.GetAsync();

        if (cookies != null)
        {
            var cookieParams = cookies.GetAllCookies().Select(c => new CookieParam
            {
                Name = c.Name,
                Value = c.Value
            }).ToArray();

            await page.SetCookieAsync(cookieParams);
        }

        await page.GoToAsync(url, WaitUntilNavigation.Networkidle2);

        //await page.WaitForNetworkIdleAsync();


        if (pageActions != null)
        {
            Logger.LogInformation("{class}.{method}: performing page actions", nameof(PuppeteerPageLoader), nameof(Load));

            for (int i = 0; i < pageActions.Count; i++)
            {
                var pageAction = pageActions[i];
                Logger.LogInformation("{class}.{method}: performing page action {current} of {count} with type {actionType}",
                    nameof(PuppeteerPageLoader),
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

    public Task<string> Load(string url, object pageActions, bool headless)
    {
        return Load(url, (List<PageAction>)pageActions, headless);
    }
}
