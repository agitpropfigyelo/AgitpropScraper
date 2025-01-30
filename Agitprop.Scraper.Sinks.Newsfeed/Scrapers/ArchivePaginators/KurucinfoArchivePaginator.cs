using System.Net;

using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ArchivePaginators;

public class KurucinfoArchivePaginator : IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        int pageNum = int.Parse(currentUrl.Split("/")[^2]) + 20;
        var result = new NewsfeedJobDescrpition
        {
            Url = new Uri($"https://kuruc.info/to/1/{pageNum}/").ToString(),
            Type = PageContentType.Archive,
        } as ScrapingJobDescription;
        return Task.FromResult(result);
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return GetNextPageAsync(currentUrl, doc);
    }
}
