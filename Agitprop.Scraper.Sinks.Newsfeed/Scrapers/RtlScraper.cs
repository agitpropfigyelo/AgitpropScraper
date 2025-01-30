using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.Rtl;

public class ArchivePaginator : IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        var url = new Uri(currentUrl);
        var newUlr = $"https://rtl.hu/legfrissebb?oldal=1";
        if (int.TryParse(url.Query.Split('=')[1], out var page))
        {
            newUlr = $"https://rtl.hu/legfrissebb?oldal={++page}";
        }
        return new NewsfeedJobDescrpition { Url = new Uri(newUlr).ToString(), Type = PageContentType.Archive };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}

public class ArchiveLinkParser : ILinkParser
{
    private readonly Uri baseUri = new Uri("https://rtl.hu");

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var jobs = doc.DocumentNode.SelectNodes("//article").Select(x => x.FirstChild.GetAttributeValue("href", ""))
                                   .Select(link => new NewsfeedJobDescrpition
                                   {
                                       Url = new Uri(baseUri, link).ToString(),
                                       Type = PageContentType.Article,
                                   })
                                   .Cast<ScrapingJobDescription>()
                                   .ToList();

        return Task.FromResult(jobs);
    }
}
