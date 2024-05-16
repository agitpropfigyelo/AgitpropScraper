using HtmlAgilityPack;
using NewsArticleScraper.Core;
using PuppeteerSharp;

namespace NewsArticleScraper.Scrapers;

public class NegynegynegyScraper : INewsSiteScraper
{
    private readonly Uri baseUrl = new("http://444.hu/");

    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='_1wnwflw0']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, '_14rkbdc0 _4r5fio3')]");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return Helper.CleanUpText(concatenatedText);
    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        Uri url = new(baseUrl, $"{dateIn.Year:D4}/{dateIn.Month:D2}/{dateIn.Day:D2}");
        try
        {
            await new BrowserFetcher().DownloadAsync();
            using IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false, // false if you need to see the browser
            });
            using IPage page = await browser.NewPageAsync();
            page.DefaultTimeout = 50000; // or you can set this as 0
            await page.GoToAsync(url.ToString(), WaitUntilNavigation.Networkidle2);
            await page.ClickAsync("#qc-cmp2-ui > div.qc-cmp2-footer.qc-cmp2-footer-overlay.qc-cmp2-footer-scrolled > div > button.css-1ruupc0");
            bool hasNext = true;
            do
            {
                try
                {
                    await page.ClickAsync("body > div.ggy75a1._11qegjk0.tz81m00._1p7jm60._2yen1f0.jm0ewz0._3h8rke0._7djmrl0._8vfv1e0.e19fld0._1v3xwj00._704pyz0.p3llho1._13jj0ut0._8b6bx79._1dy6oyqqm > div._1elk4w40._1chu0yw8.p4kpu340.p4kpu39s > div._1j78v4x3.ta3a4c20.ta3a4cbk.ta3a4cj4.ta3a4cso._1j78v4x4.ta3a4cl4.ta3a4cuo > div > div > div._15lbb9w1.ta3a4c3s.ta3a4cdc.ta3a4ckg.ta3a4cu0._1chu0ywu._1dy6oyqgp > button");
                    //await idk.EvaluateFunctionAsync("b=>b.click()",WaitUntilNavigation.Networkidle2);
                    await page.WaitForNavigationAsync();
                }
                catch (Exception ex)
                {
                    hasNext = false;
                }
            } while (hasNext);

            var htmlContent = await page.GetContentAsync();
            HtmlDocument doc = new();
            doc.LoadHtml(htmlContent);
            HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//article/div/h1/a");
            return articles.Select(x => x.GetAttributeValue("href", "")).ToList();
        }
        catch (Exception ex)
        {
            // Rethrow the exception as a task result
            throw new InvalidOperationException("Error occurred while fetching articles", ex);
            //add logging
        }
    }
}
