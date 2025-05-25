using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Scraper.Sinks.Newsfeed;
using HtmlAgilityPack;

namespace Agitprop.Sinks.Newsfeed.Scrapers.ArchivePaginators;

internal class MagyarJelenArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        var nextUrl = document.DocumentNode.SelectSingleNode("//a[contains(@class,'next page-numbers')]")?.GetAttributeValue<string>("href", "");
        return new NewsfeedJobDescrpition
        {
            Url = new Uri(nextUrl ?? GetDateBasedUrl("https://magyarjelen.hu", currentUrl)).ToString(),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, doc));
    }
}
