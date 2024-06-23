using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Scrapers.Index;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Magyarnemzet;

public class ArticleContentParser : IContentParser
{
    public Task<(string, object)> ParseContentAsync(HtmlDocument html)
    {
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = html.DocumentNode.SelectSingleNode("//h2[@class='lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        // Select nodes with tag "origo-wysiwyg-box"
        var boxNodes = html.DocumentNode.SelectNodes("//app-article-text");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        // Concatenate all text
        string concatenatedText = titleText + leadText + boxText;

        (string, object) value = ("text", Helper.CleanUpText(concatenatedText));
        return Task.FromResult(value);
    }
    public Task<(string, object)> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }

}

public class ArchivePaginator : IPaginator
{
    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var uri = new Uri(currentUrl);
        var currentDate = DateOnly.ParseExact(uri.Segments[^1].Replace("_sitemap.xml", ""), "yyyyMM");
        var nextJobDate = currentDate.AddMonths(-1);
        return Task.FromResult(new ScrapingJobBuilder().SetUrl($"{uri.GetLeftPart(UriPartial.Authority)}/{nextJobDate:yyyyMM}_sitemap.xml")
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddContentParser(new ArticleContentParser())
                                       .AddPagination(new ArchivePaginator())
                                       .Build());
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetNextPageAsync(currentUrl, doc);
    }
}

public class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = base.GetLinks(doc.ToString()).Select(link => new ScrapingJobBuilder().SetUrl(link)
                                                                                .SetPageType(PageType.Static)
                                                                                .SetPageCategory(PageCategory.TargetPage)
                                                                                .AddContentParser(new ArticleContentParser())
                                                                                .Build()).ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = base.GetLinks(docString).Select(link => new ScrapingJobBuilder().SetUrl(link)
                                                                                .SetPageType(PageType.Static)
                                                                                .SetPageCategory(PageCategory.TargetPage)
                                                                                .AddContentParser(new ArticleContentParser())
                                                                                .Build()).ToList();
        return Task.FromResult(result);
    }
}
