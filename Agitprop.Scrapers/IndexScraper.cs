using System.Globalization;
using System.Xml;
using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
using HtmlAgilityPack;


namespace Agitprop.Scrapers.Index;

public class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("//*[@id='content']/div[4]/div[1]/div/div[1]/div[2]/span");
        DateTime date = DateTime.Parse(dateNode.InnerText);

        var titleNode = html.DocumentNode.SelectSingleNode("//div[@class='content-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = html.DocumentNode.SelectSingleNode("//div[@class='lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        var boxNode = html.DocumentNode.SelectSingleNode("//div[@class='cikk-torzs']");

        var toRemove = boxNode.SelectNodes("//div[contains(@class, 'cikk-bottom-text-ad')]");
        foreach (var item in toRemove)
        {
            item.Remove();
        }

        string boxText = boxNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + leadText + boxText;

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
        return this.ParseContentAsync(doc);
    }
}

public class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = GetLinks(docString).Select(link => new ScrapingJobBuilder().SetUrl(link)
                                                    .SetPageType(PageType.Static)
                                                    .SetPageCategory(PageCategory.TargetPage)
                                                    .AddContentParser(new ArticleContentParser())
                                                    .Build()).ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        return this.GetLinksAsync(baseUrl, doc.ToString());
    }
}

public class ArchivePaginator : IPaginator
{
    public ScrapingJob GetNextPage(string currentUrl, HtmlDocument document)
    {
        var uri = new Uri(currentUrl);
        var currentDate = DateOnly.ParseExact(uri.Segments[^1].Replace("cikkek_", "").Replace(".xml", ""), "yyyyMM");
        var nextJobDate = currentDate.AddMonths(-1);
        return new ScrapingJobBuilder().SetUrl($"{uri.GetLeftPart(UriPartial.Authority)}/sitemap/cikkek_{nextJobDate:yyyyMM}.xml")
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddContentParser(new ArticleContentParser())
                                       .AddPagination(new ArchivePaginator())
                                       .Build();
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}
