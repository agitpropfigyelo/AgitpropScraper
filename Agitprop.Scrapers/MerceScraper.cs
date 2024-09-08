using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Merce;

public class ArticleContentParser : IContentParser
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

public class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//article/a");
        var result = articles.Select(x => x.GetAttributeValue("href", "")).Select(link =>
        {
            return new ScrapingJobBuilder().SetUrl(link)
                                           .SetPageType(PageType.Static)
                                           .SetPageCategory(PageCategory.TargetPage)
                                           .AddContentParser(new ArticleContentParser())
                                           .Build();
        }).ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        return this.GetLinksAsync(baseUrl, doc);
    }
}

public class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJob GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new ScrapingJobBuilder().SetUrl(GetDateBasedUrl("https://merce.hu", currentUrl))
                                   .SetPageType(PageType.Static)
                                   .SetPageCategory(PageCategory.PageWithPagination)
                                   .AddPagination(new ArchivePaginator())
                                   .AddLinkParser(new ArchiveLinkParser())
                                   .Build();
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}
