using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Alfahir;
public class ArticleContentParser : IContentParser
{
    public Task<(string, object)> ParseContentAsync(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='article-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = document.DocumentNode.SelectSingleNode("//p[@class='article-lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        // Select nodes with tag "origo-wysiwyg-box"
        var articleNode = document.DocumentNode.SelectNodes("//div[contains(@class, 'article-content')]")[1];
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + leadText + articleText;
        (string, object) result = ("text", Helper.CleanUpText(concatenatedText));
        return Task.FromResult(result);
    }

    public Task<(string, object)> ParseContentAsync(string html)
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
        List<ScrapingJob> jobs = [];
        HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes(".//div[@class='article']");
        foreach (var item in articleNodes)
        {
            jobs.Add(CreateJob(item));
        }
        return Task.FromResult(jobs);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        List<ScrapingJob> jobs = [];
        HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes(".//div[@class='article']");
        foreach (var item in articleNodes)
        {
            jobs.Add(CreateJob(item));
        }
        return Task.FromResult(jobs);
    }

    private ScrapingJob CreateJob(HtmlNode nodeIn)
    {
        var link = nodeIn.SelectSingleNode(".//a[@class='article-title-link']").GetAttributeValue<string>("href", "");
        var builder = new ScrapingJobBuilder().SetUrl(link)
                                              .SetPageCategory(PageCategory.TargetPage)
                                              .SetPageType(PageType.Static)
                                              .AddContentParser(new ArticleContentParser())
                                              .AddLinkParser(new ArchiveLinkParser());

        return builder.Build();
    }
}

public class ArchivePaginator : IPaginator
{
    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var url = "https://alfahir.hu/hirek/oldalak/1";
        if (int.TryParse(new Uri(currentUrl).Segments.Last(), out var counter))
        {
            url = $"https://alfahir.hu/hirek/oldalak/{++counter}";
        }
        var result = new ScrapingJobBuilder().SetUrl(url)
                                             .SetPageType(PageType.Static)
                                             .SetPageCategory(PageCategory.PageWithPagination)
                                             .AddPagination(new ArchivePaginator())
                                             .AddLinkParser(new ArchiveLinkParser())
                                             .Build();
        return Task.FromResult(result);
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        var url = "https://alfahir.hu/hirek/oldalak/1";
        if (int.TryParse(new Uri(currentUrl).Segments.Last(), out var counter))
        {
            url = $"https://alfahir.hu/hirek/oldalak/{++counter}";
        }
        var result = new ScrapingJobBuilder().SetUrl(url)
                                             .SetPageType(PageType.Static)
                                             .SetPageCategory(PageCategory.PageWithPagination)
                                             .AddPagination(new ArchivePaginator())
                                             .AddLinkParser(new ArchiveLinkParser())
                                             .Build();
        return Task.FromResult(result);
    }
}