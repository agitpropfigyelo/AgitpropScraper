using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
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