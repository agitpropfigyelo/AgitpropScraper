using Agitprop.Core;
using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;


namespace Agitprop.Scrapers.Index;

internal class ArticleContentParser : IContentParser
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
            SourceSite = NewsSites.Index,
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

internal class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = GetLinks(docString).Select(link => new ScrapingJobDescription
        {
            Url = new Uri(link),
            Type = PageContentType.Article,
        }).ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        return this.GetLinksAsync(baseUrl, doc.ToString());
    }
}

internal class ArchivePaginator : IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        var uri = new Uri(currentUrl);
        var currentDate = DateOnly.ParseExact(uri.Segments[^1].Replace("cikkek_", "").Replace(".xml", ""), "yyyyMM");
        var nextJobDate = currentDate.AddMonths(-1);
        return new ScrapingJobDescription
        {
            Url = new Uri($"{uri.GetLeftPart(UriPartial.Authority)}/sitemap/cikkek_{nextJobDate:yyyyMM}.xml"),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}
