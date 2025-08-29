using Agitprop.Core.Enums;
using Agitprop.Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;

namespace Agitprop.Infrastructure.PageLoader;

/// <summary>
/// An abstract base class for loading web pages using a browser.
/// </summary>
public abstract class BrowserPageLoader
{
    /// <summary>
    /// A dictionary mapping page action types to their corresponding execution logic.
    /// </summary>
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
    /// Initializes a new instance of the <see cref="BrowserPageLoader"/> class.
    /// </summary>
    /// <param name="logger">The logger for logging information and errors.</param>
    protected BrowserPageLoader(ILogger<BrowserPageLoader>? logger = default)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets the logger for logging information and errors.
    /// </summary>
    protected ILogger<BrowserPageLoader>? Logger { get; }
}
