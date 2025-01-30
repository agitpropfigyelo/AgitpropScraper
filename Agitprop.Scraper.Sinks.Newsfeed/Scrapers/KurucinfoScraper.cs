using System.Net;

using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.Kurucinfo;

internal class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var jobs = doc.DocumentNode.SelectNodes(".//div[@class='alcikkheader']/a")
                                   .Select(x => x.GetAttributeValue<string>("href", ""))
                                   .Select(url => new NewsfeedJobDescrpition
                                   {
                                       Url = new Uri(url).ToString(),
                                       Type = PageContentType.Article,
                                   }).Cast<ScrapingJobDescription>().ToList();

        return Task.FromResult(jobs);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}

public class ArchivePaginator : IPaginator
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
        return this.GetNextPageAsync(currentUrl, doc);
    }
}
