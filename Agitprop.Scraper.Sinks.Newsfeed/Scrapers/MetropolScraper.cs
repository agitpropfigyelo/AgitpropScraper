using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.Metropol;

internal class ArchivePaginator : SitemapArchivePaginator, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new NewsfeedJobDescrpition
        {
            Url = new Uri(GetUrl(currentUrl, document)).ToString(),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, doc));
    }
}

internal class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = GetLinks(docString).Select(link => new NewsfeedJobDescrpition
        {
            Url = new Uri(link).ToString(),
            Type = PageContentType.Article,
        }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = GetLinks(doc.ToString()).Select(link => new NewsfeedJobDescrpition
        {
            Url = new Uri(link).ToString(),
            Type = PageContentType.Article,
        }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }
}
