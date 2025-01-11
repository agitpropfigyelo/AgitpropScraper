using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;

namespace Agitprop.Infrastructure.PageLoader;

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
    protected BrowserPageLoader(ILogger<BrowserPageLoader> logger)
    {
        Logger = logger;
    }
    protected ILogger<BrowserPageLoader> Logger { get; }
}
