using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;
using PuppeteerSharp;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Negynegynegy;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        //TODO: improve date parsing (today vs this year vs last year...)
        var dateNode = html.DocumentNode.SelectSingleNode("/html/body/div[1]/div[1]/div[3]/div[4]/div/div[2]/div[2]");
        if (!DateTime.TryParse(dateNode.InnerText, out DateTime date))
        {
            DateTime.Parse($"{DateTime.Now.Year}. {dateNode.InnerText}");
        }
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("/html/body/div[1]/div[1]/div[3]/div[3]/h1");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        // /html/body/div[1]/div[1]/div[3]/div[5]/div/div[2]
        // /html/body/div[1]/div[1]/div[3]/div[5]/div/div[2]
        var articleNodes = html.DocumentNode.SelectNodes("/html/body/div[1]/div[1]/div[3]/div[5]/div/div[2]//p");
        var articleText = string.Join(" ", articleNodes.Select(x => x.InnerText.Trim()));

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.NegyNegyNegy,
            Text = Helper.CleanUpText(concatenatedText)
        });
    }

    public Task<ContentParserResult> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return ParseContentAsync(doc);
    }
}

internal class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new NewsfeedJobDescrpition
        {
            Url = new Uri(GetDateBasedUrl("https://444.hu", currentUrl)).ToString(),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}

internal class ArchiveScrollAction : IBrowserAction
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

internal class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//article/div/h1/a");
        var result = articles.Select(x => x.GetAttributeValue("href", ""))
                             .Select(link => new NewsfeedJobDescrpition
                             {
                                 Url = new Uri($"https://444.hu{link}").ToString(),
                                 Type = PageContentType.Article,
                             }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }
}