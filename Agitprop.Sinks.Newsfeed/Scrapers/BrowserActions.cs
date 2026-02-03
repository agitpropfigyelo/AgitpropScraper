using Agitprop.Core.Interfaces;

using PuppeteerSharp;

namespace Agitprop.Sinks.Newsfeed.Scrapers;

/// <summary>
/// Represents a browser action for scrolling through the Negynegynegy archive pages.
/// </summary>
internal class NegynegynegyArchiveScrollAction : IBrowserAction
{
    /// <summary>
    /// Executes the scrolling action on the specified browser page.
    /// </summary>
    /// <param name="page">The browser page to perform the action on.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ExecuteAsync(IPage page)
    {
        //Accept GDPR cookies if present
        var cookieAcceptSelector = "#accept-btn";
        await page.WaitForSelectorAsync(cookieAcceptSelector);
        await page.ClickAsync(cookieAcceptSelector);
        bool hasNext = true;
        do
        {
            try
            {
                var loadBtnSelector = "#ember4";
                var btn = await page.QuerySelectorAsync(loadBtnSelector);
                await btn.ClickAsync();
                // Click the button
                await page.WaitForNetworkIdleAsync();
            }
            catch (Exception)
            {
                hasNext = false;
            }
        } while (hasNext);
    }
}
