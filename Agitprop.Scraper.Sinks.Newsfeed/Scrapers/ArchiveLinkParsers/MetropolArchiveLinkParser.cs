using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.Metropol;

internal class MetropolArchiveLinkParser : SitemapLinkParser, ILinkParser
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
