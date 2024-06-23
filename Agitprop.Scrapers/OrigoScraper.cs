using HtmlAgilityPack;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Infrastructure.Enums;

namespace Agitprop.Scrapers.Origo;

public class ArticleContentParser : IContentParser
{
    public (string, object) ParseContent(HtmlDocument html)
    {
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='article-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = html.DocumentNode.SelectSingleNode("//div[@class='article-lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        var boxNodes = html.DocumentNode.SelectNodes("//origo-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        string concatenatedText = titleText + leadText + boxText;

        return ("text", Helper.CleanUpText(concatenatedText));
    }

    public Task<(string, object)> ParseContentAsync(HtmlDocument html)
    {
        return Task.FromResult(ParseContent(html));
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
    private readonly Uri baseUri = new Uri("https://www.origo.hu");

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);

    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var hrefs = doc.DocumentNode.Descendants("article")
                   .Select(article => article.Descendants("a").FirstOrDefault())
                   .Where(a => a != null)
                   .Select(a => a.GetAttributeValue("href", ""))
                   .ToList();
        var result = hrefs.Select(link => new Uri(baseUri, link).ToString()).Select(link =>
        {
            return new ScrapingJobBuilder().SetUrl(link)
                                           .SetPageType(PageType.Static)
                                           .SetPageCategory(PageCategory.TargetPage)
                                           .Build();
        }).ToList();
        return Task.FromResult(result);
    }
}

public class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJob GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new ScrapingJobBuilder().SetUrl(GetDateBasedUrl("https://www.origo.hu/hirarchivum", currentUrl))
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
    protected static new string GetDateBasedUrl(string urlBase, string current)
    {
        var currentUrl = new Uri(current);
        var nextDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var toParse = currentUrl.Segments[2..5].ToList();

        if (DateOnly.TryParse(string.Join(".", toParse.Select(x => x.Replace("/", ""))), out DateOnly date))
        {
            nextDate = date.AddDays(-1);
        }
        return $"{urlBase}/{nextDate.Year:D4}/{nextDate.Month:D2}/{nextDate.Day:D2}";
    }
}
