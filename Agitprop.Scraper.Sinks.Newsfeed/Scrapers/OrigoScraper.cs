using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.Origo;
internal class ArchiveLinkParser : ILinkParser
{
    private readonly Uri baseUri = new Uri("https://www.origo.hu");

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);

    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var hrefs = doc.DocumentNode.Descendants("article")
                   .Select(article => article.Descendants("a").FirstOrDefault())
                   .Where(a => a != null)
                   .Select(a => a.GetAttributeValue("href", ""))
                   .ToList();
        var result = hrefs.Select(link => new Uri(baseUri, link).ToString())
                          .Select(link => new NewsfeedJobDescrpition
                          {
                              Url = new Uri(link).ToString(),
                              Type = PageContentType.Article,
                          }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }
}

internal class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new NewsfeedJobDescrpition
        {
            Url = new Uri(GetDateBasedUrl("https://www.origo.hu/hirarchivum", currentUrl)).ToString(),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
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
