using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Pestisracok;

internal class ArchivePaginator : DateBasedArchive, IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        return Task.FromResult(new NewsfeedJobDescrpition
        {
            Url = new Uri(GetDateBasedUrl("https://www.pestisracok.hu", currentUrl)).ToString(),
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
internal class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = doc.DocumentNode.SelectNodes("//*[@id='home-widget-wrap']/div/ul/li/div[1]/a")
                                     .Select(x => x.GetAttributeValue("href", ""))
                                     .Select(link => new NewsfeedJobDescrpition
                                     {
                                         Url = new Uri(link).ToString(),
                                         Type = PageContentType.Article,
                                     }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}
