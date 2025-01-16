using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Magyarnemzet;

internal class ArchivePaginator : IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var uri = new Uri(currentUrl);
        var currentDate = DateOnly.ParseExact(uri.Segments[^1].Replace("_sitemap.xml", ""), "yyyyMM");
        var nextJobDate = currentDate.AddMonths(-1);
        return Task.FromResult(new NewsfeedJobDescrpition
        {
            Url = new Uri($"{uri.GetLeftPart(UriPartial.Authority)}/{nextJobDate:yyyyMM}_sitemap.xml").ToString(),
            Type = PageContentType.Archive,

        } as ScrapingJobDescription);
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetNextPageAsync(currentUrl, doc);
    }
}

internal class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = base.GetLinks(doc.ToString())
                         .Select(link => new NewsfeedJobDescrpition
                         {
                             Url = new Uri(link).ToString(),
                             Type = PageContentType.Article,

                         }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = base.GetLinks(docString)
                        .Select(link => new NewsfeedJobDescrpition
                        {
                            Url = new Uri(link).ToString(),
                            Type = PageContentType.Article,
                        }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }
}
