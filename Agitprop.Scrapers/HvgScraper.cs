using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;
using PuppeteerSharp;


namespace Agitprop.Scrapers.Hvg;

public class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJob GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new ScrapingJobBuilder().SetUrl(GetDateBasedUrl("http://hvg.hu/frisshirek", currentUrl))
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddPagination(new ArchivePaginator())
                                       .AddLinkParser(new ArchiveLinkParser())
                                       .AddPageAction(new PageAction(PageActionType.Click, ["#qc-cmp2-ui > div.qc-cmp2-footer.qc-cmp2-footer-overlay.qc-cmp2-footer-scrolled > div > button:nth-child(2)"]))
                                       .AddPageAction(new PageAction(PageActionType.ScrollToEnd))
                                       .Build();
    }

    protected static new string GetDateBasedUrl(string urlBase, string current)
    {
        var currentUrl = new Uri(current);
        var nextDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        if (DateOnly.TryParse(string.Join(".", currentUrl.Segments[^1]), out DateOnly date))
        {
            nextDate = date.AddDays(-1);
        }
        return $"{urlBase}/{nextDate.Year:D4}.{nextDate.Month:D2}.{nextDate.Day:D2}";
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}

public class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var articleUrls = doc.DocumentNode.SelectNodes("//article/div/h1/a").Select(x => x.GetAttributeValue("href", "")).ToList();
        var result = articleUrls.Select(static url =>
        {
            return new ScrapingJobBuilder().SetUrl(url)
                                           .SetPageType(PageType.Static)
                                           .SetPageCategory(PageCategory.TargetPage)
                                           .AddContentParser(new ArticleContentParser())
                                           .Build();
        }).ToList();
        return Task.FromResult(result);
    }
}

public class ArticleContentParser : IContentParser
{
    public (string, object) ParseContent(HtmlDocument html)
    {
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//div[@class='article-title article-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = html.DocumentNode.SelectSingleNode("//p[contains(@class, 'article-lead entry-summary')]");
        string leadText = leadNode.InnerText.Trim() + " ";

        var articleNode = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'article-content entry-content')]");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + leadText + articleText;

        return ("text", Helper.CleanUpText(concatenatedText));
    }

    public Task<(string, object)> ParseContentAsync(HtmlDocument html)
    {
        return Task.FromResult(this.ParseContent(html));
    }

    public Task<(string, object)> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }
}


public class HvgScraper
{
    private readonly Uri baseUrl = new("http://hvg.hu/");

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
