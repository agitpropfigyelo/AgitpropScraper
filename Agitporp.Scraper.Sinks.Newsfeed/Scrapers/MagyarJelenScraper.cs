using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Magyarjelen;

internal class ArchivePaginator : DateBasedArchive, IPaginator
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
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}

internal class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//div[@class='col-8 main-content']/section/div/div/article/div[1]/a");
        var idk = articles.Select(x => x.GetAttributeValue("href", ""))
                          .Select(url => new NewsfeedJobDescrpition
                          {
                              Url = new Uri(url).ToString(),
                              Type = PageContentType.Article,
                          }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(idk);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}
