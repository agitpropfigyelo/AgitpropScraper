using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.Alfahir;

internal class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        List<ScrapingJobDescription> jobs = [];
        HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes(".//div[@class='article']");
        foreach (var item in articleNodes)
        {
            jobs.Add(CreateJob(item));
        }
        return Task.FromResult(jobs);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        List<ScrapingJobDescription> jobs = [];
        HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes(".//div[@class='article']");
        foreach (var item in articleNodes)
        {
            jobs.Add(CreateJob(item));
        }
        return Task.FromResult(jobs);
    }

    private ScrapingJobDescription CreateJob(HtmlNode nodeIn)
    {
        var link = nodeIn.SelectSingleNode(".//a[@class='article-title-link']").GetAttributeValue<string>("href", "");

        return new NewsfeedJobDescrpition
        {
            Url = new Uri($"https://alfahir.hu{link}").ToString(),
            Type = PageContentType.Article,
        };
    }
}

internal class ArchivePaginator : IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var url = "https://alfahir.hu/hirek/oldalak/1";
        if (int.TryParse(new Uri(currentUrl).Segments.Last(), out var counter))
        {
            url = $"https://alfahir.hu/hirek/oldalak/{++counter}";
        }
        var result = new NewsfeedJobDescrpition
        {
            Url = new Uri(url).ToString(),
            Type = PageContentType.Archive,
        } as ScrapingJobDescription;
        return Task.FromResult(result);
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        var url = "https://alfahir.hu/hirek/oldalak/1";
        if (int.TryParse(new Uri(currentUrl).Segments.Last(), out var counter))
        {
            url = $"https://alfahir.hu/hirek/oldalak/{++counter}";
        }
        var result = new NewsfeedJobDescrpition
        {
            Url = new Uri(url).ToString(),
            Type = PageContentType.Archive,
        } as ScrapingJobDescription;
        return Task.FromResult(result);
    }
}
