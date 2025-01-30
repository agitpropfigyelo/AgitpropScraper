using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.ArchiveLinkParsers;

internal class NegynegynegyArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return GetLinksAsync(baseUrl, doc);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//article/div/h1/a");
        var result = articles.Select(x => x.GetAttributeValue("href", ""))
                             .Select(link => new NewsfeedJobDescrpition
                             {
                                 Url = new Uri($"https://444.hu{link}").ToString(),
                                 Type = PageContentType.Article,
                             }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }
}
