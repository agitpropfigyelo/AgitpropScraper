using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Huszonnegy;

internal class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var nodes = doc.DocumentNode.SelectNodes("//*[@id='content']/h2/a");
        var result = nodes.Select(x => x.GetAttributeValue("href", ""))
                          .Select(url => new NewsfeedJobDescrpition
                          {
                              Url = new Uri(url).ToString(),
                              Type = PageContentType.Article,
                          }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}

internal class ArchivePaginator : DateBasedArchive, IPaginator
{
    public async Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var url = GetDateBasedUrl("https://24.hu", currentUrl);
        var job = new NewsfeedJobDescrpition
        {
            Url = new Uri(url).ToString(),
            Type = PageContentType.Archive,
        };
        return await Task.FromResult(job);
    }

    public async Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return await GetNextPageAsync(currentUrl, doc);
    }
}
