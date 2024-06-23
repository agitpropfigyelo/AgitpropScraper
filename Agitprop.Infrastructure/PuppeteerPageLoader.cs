using System.Reflection;
using Agitprop.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace Agitprop.Infrastructure;

public class PuppeteerPageLoader : BrowserPageLoader, IBrowserPageLoader
{
    private readonly ICookiesStorage _cookiesStorage;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public PuppeteerPageLoader(ILogger logger, ICookiesStorage cookiesStorage) : base(logger)
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
            await browserFetcher.DownloadAsync(BrowserTag.Latest);
            Logger.LogInformation("{class}.{method}: Browser is downloaded", nameof(PuppeteerPageLoader), nameof(Load));
        }
        finally
        {
            _semaphore.Release();
        }
        PuppeteerSharp.BrowserData.InstalledBrowser idk = browserFetcher.GetInstalledBrowsers().First();
        Logger.LogInformation("{class}.{method}: Launching a browser", nameof(PuppeteerPageLoader), nameof(Load));
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = headless,
            ExecutablePath = browserFetcher.GetInstalledBrowsers().First().GetExecutablePath(),
        });

        Logger.LogInformation("{class}.{method}: creating a new page", nameof(PuppeteerPageLoader), nameof(Load));
        await using var page = await browser.NewPageAsync();

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

        await page.GoToAsync(url, WaitUntilNavigation.DOMContentLoaded);

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
        return this.Load(url, (List<PageAction>)pageActions, headless);
    }
}
public abstract class BrowserPageLoader
{
    protected readonly Dictionary<PageActionType, Func<IPage, object[], Task>> PageActions = new()
    {
        {
            PageActionType.ScrollToEnd,
            async (page, _) => await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight);")
        },
        { PageActionType.Wait, async (_, data) => await Task.Delay(Convert.ToInt32(data.First())) },
        { PageActionType.WaitForNetworkIdle, async (page, _) => await page.WaitForNetworkIdleAsync() },
        { PageActionType.Click, async (page, data) => await page.ClickAsync((string)data.First()) },
        { PageActionType.Execute, async (page, action) => await ((IBrowserAction)action.First()).ExecuteAsync(page) },
    };

    /// <summary>
    ///     Constructor that takes ILogger argument
    /// </summary>
    /// <param name="logger"></param>
    protected BrowserPageLoader(ILogger logger)
    {
        Logger = logger;
    }
    protected ILogger Logger { get; }
}

public enum PageActionType
{
    Click,
    Wait,
    ScrollToEnd,
    Execute,
    EvaluateExpression,
    WaitForSelector,
    WaitForNetworkIdle
}

public record PageAction(PageActionType Type, params object[] Parameters);