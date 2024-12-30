using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Hvg;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("//*[@id='perspective']/div/div/main/div[1]/div/div/div[1]/div[2]/div[1]/div/time[1]");
        DateTime date = DateTime.Parse(dateNode.GetAttributeValue("datetime", ""));


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

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.HVG,
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
    public NewsfeedJobDescrpition GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new NewsfeedJobDescrpition
        {
            Url = new Uri(GetDateBasedUrl("http://hvg.hu/frisshirek", currentUrl)),
            Type = PageContentType.Archive,
        };
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

    public Task<NewsfeedJobDescrpition> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, doc));
    }
}

internal class ArchiveLinkParser : ILinkParser
{
    private readonly Uri baseUri = new Uri("https://www.hvg.hu");

    public Task<List<NewsfeedJobDescrpition>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return GetLinksAsync(baseUrl, doc);
    }

    public Task<List<NewsfeedJobDescrpition>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var articleUrls = doc.DocumentNode.SelectNodes("//article/div/h1/a").Select(x => x.GetAttributeValue("href", "")).ToList();
        var result = articleUrls.Select(link => new NewsfeedJobDescrpition
        {
            Url = new Uri(baseUri, link),
            Type = PageContentType.Article,
        }).ToList();
        return Task.FromResult(result);
    }
}