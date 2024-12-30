using Agitprop.Core;
using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Merce;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("/html/body/div[4]/div/div/main/div/div[2]/div/article/div/div[1]/div[1]/div[2]/div[2]/time");
        DateTime date = DateTime.Parse(dateNode.GetAttributeValue("datetime", ""));
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='entry-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var articleNode = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'entry-content')]");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.Merce,
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

internal class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//article/a");
        var result = articles.Select(x => x.GetAttributeValue("href", ""))
                             .Select(link => new ScrapingJobDescription
                             {
                                 Url = new Uri(link),
                                 Type = PageContentType.Article,

                             })
                             .ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        return this.GetLinksAsync(baseUrl, doc);
    }
}

internal class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new ScrapingJobDescription
        {
            Url = new Uri(GetDateBasedUrl("https://merce.hu", currentUrl)),
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
