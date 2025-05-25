using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Scraper.Sinks.Newsfeed;
using HtmlAgilityPack;

namespace Agitprop.Sinks.Newsfeed.Scrapers.ArchiveLinkParsers;

internal class MagyarJelenArchiveLinkParser : ILinkParser
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
        return GetLinksAsync(baseUrl, doc);
    }
}
