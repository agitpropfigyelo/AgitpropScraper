using Agitprop.Core;
using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Magyarnemzet;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("/html/body/app-root/app-base/div[2]/div/app-slug-route-handler/app-article/section/div/div[1]/app-article-header/div/div[3]/div[1]/div/span[2]");
        DateTime date = DateTime.Parse(dateNode.InnerText);
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = html.DocumentNode.SelectSingleNode("//h2[@class='lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        // Select nodes with tag "origo-wysiwyg-box"
        var boxNodes = html.DocumentNode.SelectNodes("//app-article-text");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        // Concatenate all text
        string concatenatedText = titleText + leadText + boxText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.MagyarNemzet,
            Text = Helper.CleanUpText(concatenatedText)
        });
    }

    public Task<ContentParserResult> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }

}

internal class ArchivePaginator : IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var uri = new Uri(currentUrl);
        var currentDate = DateOnly.ParseExact(uri.Segments[^1].Replace("_sitemap.xml", ""), "yyyyMM");
        var nextJobDate = currentDate.AddMonths(-1);
        return Task.FromResult(new ScrapingJobDescription
        {
            Url = new Uri($"{uri.GetLeftPart(UriPartial.Authority)}/{nextJobDate:yyyyMM}_sitemap.xml"),
            Type = PageContentType.Archive,

        });
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetNextPageAsync(currentUrl, doc);
    }
}

internal class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = base.GetLinks(doc.ToString())
                         .Select(link => new ScrapingJobDescription
                         {
                             Url = new Uri(link),
                             Type = PageContentType.Article,

                         })
                         .ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = base.GetLinks(docString)
                        .Select(link => new ScrapingJobDescription
                        {
                            Url = new Uri(link),
                            Type = PageContentType.Article,
                        })
                        .ToList();
        return Task.FromResult(result);
    }
}
