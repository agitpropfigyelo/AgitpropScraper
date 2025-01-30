using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.Ripost;

internal class RipostArchivePaginator : SitemapArchivePaginator, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new NewsfeedJobDescrpition
        {
            Url = new Uri(base.GetUrl(currentUrl, document)).ToString(),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument html = new();
        html.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, html));

    }
}
