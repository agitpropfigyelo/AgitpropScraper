using Agitprop.Core;
using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Magyarjelen;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("/html/body/div[1]/div[5]/div[1]/div/div/span[2]/time");
        DateTime date = DateTime.Parse(dateNode.InnerText);

        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='is-title post-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNode = html.DocumentNode.SelectSingleNode("//div[@class='post-content cf entry-content content-spacious']");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.MagyarJelen,
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

internal class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        var nextUrl = document.DocumentNode.SelectSingleNode("//a[contains(@class,'next page-numbers')]")?.GetAttributeValue<string>("href", "");
        return new ScrapingJobDescription
        {
            Url = new Uri(nextUrl ?? GetDateBasedUrl("https://magyarjelen.hu", currentUrl)),
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

internal class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//div[@class='col-8 main-content']/section/div/div/article/div[1]/a");
        var idk = articles.Select(x => x.GetAttributeValue("href", ""))
                          .Select(url => new ScrapingJobDescription
                          {
                              Url = new Uri(url),
                              Type = PageContentType.Article,
                          })
                          .ToList();
        return Task.FromResult(idk);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}