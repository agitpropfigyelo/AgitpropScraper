using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Magyarjelen;


public class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJob GetNextPage(string currentUrl, HtmlDocument document)
    {
        var nextUrl = document.DocumentNode.SelectSingleNode("//a[contains(@class,'next page-numbers')]")?.GetAttributeValue<string>("href", "");
        return new ScrapingJobBuilder().SetUrl(nextUrl ?? GetDateBasedUrl("https://magyarjelen.hu", currentUrl))
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddPagination(new ArchivePaginator())
                                       .AddLinkParser(new ArchiveLinkParser())
                                       .Build();
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}
public class ArticleContentParser : IContentParser
{
    public (string, object) ParseContent(HtmlDocument html)
    {
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='is-title post-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNode = html.DocumentNode.SelectSingleNode("//div[@class='post-content cf entry-content content-spacious']");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return ("text", Helper.CleanUpText(concatenatedText));
    }

    public Task<(string, object)> ParseContentAsync(HtmlDocument html)
    {
        return Task.FromResult(this.ParseContent(html));
    }
        public Task<(string, object)> ParseContentAsync(string html)
    {
        var doc =new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }
}

public class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//div[@class='col-8 main-content']/section/div/div/article/div[1]/a");
        var idk = articles.Select(x => x.GetAttributeValue("href", "")).Select(url =>
        {
            return new ScrapingJobBuilder().SetUrl(url)
                                                  .SetPageCategory(PageCategory.TargetPage)
                                                  .SetPageType(PageType.Static)
                                                  .AddContentParser(new ArticleContentParser())
                                                  .Build();
        }).ToList();
        return Task.FromResult(idk);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}