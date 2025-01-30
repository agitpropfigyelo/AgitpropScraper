using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers.Merce;

internal class MerceArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//article/a");
        var result = articles.Select(x => x.GetAttributeValue("href", ""))
                             .Select(link => new NewsfeedJobDescrpition
                             {
                                 Url = new Uri(link).ToString(),
                                 Type = PageContentType.Article,

                             }).Cast<ScrapingJobDescription>().ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        return this.GetLinksAsync(baseUrl, doc);
    }
}
