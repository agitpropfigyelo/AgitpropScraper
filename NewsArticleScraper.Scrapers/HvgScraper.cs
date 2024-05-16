using HtmlAgilityPack;
using NewsArticleScraper.Core;
using PuppeteerSharp;
using PuppeteerSharp.Input;

namespace NewsArticleScraper.Scrapers;

public class HvgScraper : INewsSiteScraper
{
    private readonly Uri baseUrl = new("http://hvg.hu/");
    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//div[@class='article-title article-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = document.DocumentNode.SelectSingleNode("//p[contains(@class, 'article-lead entry-summary')]");
        string leadText = leadNode.InnerText.Trim() + " ";

        var articleNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'article-content entry-content')]");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + leadText + articleText;

        return Helper.CleanUpText(concatenatedText);
    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        Uri url = new(baseUrl, $"frisshirek/{dateIn.Year:D4}.{dateIn.Month:D2}.{dateIn.Day:D2}");
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
            bool hasNext = true;
            do
            {
                try
                {
                    await page.ClickAsync("#qc-cmp2-ui > div.qc-cmp2-footer.qc-cmp2-footer-overlay.qc-cmp2-footer-scrolled > div > button:nth-child(2)");
                    await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight)");
                    await page.ClickAsync("div.latest-nav > div:first-of-type > a");
                    await page.WaitForNavigationAsync();
                    //await idk.EvaluateFunctionAsync("b=>b.click()",WaitUntilNavigation.Networkidle2);
                }
                catch (Exception ex)
                {
                    hasNext = false;
                    throw;
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
