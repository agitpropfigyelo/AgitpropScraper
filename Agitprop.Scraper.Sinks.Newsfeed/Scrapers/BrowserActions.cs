using Agitprop.Core.Interfaces;

using PuppeteerSharp;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers;

internal class NegynegynegyArchiveScrollAction : IBrowserAction
{
    public async Task ExecuteAsync(IPage page)
    {

        await page.WaitForSelectorAsync("#qc-cmp2-ui > div.qc-cmp2-footer.qc-cmp2-footer-overlay.qc-cmp2-footer-scrolled > div > button.css-1ruupc0");
        await page.ClickAsync("#qc-cmp2-ui > div.qc-cmp2-footer.qc-cmp2-footer-overlay.qc-cmp2-footer-scrolled > div > button.css-1ruupc0");
        bool hasNext = true;
        do
        {
            try
            {
                var btn = await page.QuerySelectorAsync("body > div.ggy75a1._11qegjk0.tz81m00._1p7jm60._2yen1f0.jm0ewz0._3h8rke0._7djmrl0._8vfv1e0.e19fld0._1v3xwj00._704pyz0.p3llho1._13jj0ut0._8b6bxc9._1dy6oyqvm > div._1igl1k0._1chu0yw8.p4kpu340.p4kpu39s > div._148bpe33.ta3a4c20.ta3a4cbk.ta3a4cj4.ta3a4cso._148bpe34.ta3a4cl4.ta3a4cuo > div._148bpe33.ta3a4c20.ta3a4cbk.ta3a4cj4.ta3a4cso._148bpe36 > div > div.ta3a4c3s.ta3a4cdc.ta3a4ckg.ta3a4cu0._1chu0yww.ta3a4c24w._1dy6oyqgp > button");
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
