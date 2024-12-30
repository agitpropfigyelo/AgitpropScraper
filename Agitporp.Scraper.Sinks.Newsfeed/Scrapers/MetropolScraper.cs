using Agitporp.Scraper.Sinks.Newsfeed.Scrapers;
using Agitprop.Core;
using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Metropol;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("/html/body/app-root/app-base/div/app-article-page/section/div[1]/div/metropol-article-header/section/div[5]/div[2]/div");
        DateTime date = DateTime.Parse(dateNode.InnerText.Split(':')[1]);

        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[contains(@class, 'article-header-title')]");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = html.DocumentNode.SelectSingleNode("//p[contains(@class, 'article-header-lead')]");
        string leadText = leadNode.InnerText.Trim() + " ";

        // Select nodes with tag "origo-wysiwyg-box"
        var boxNodes = html.DocumentNode.SelectNodes("//metropol-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        // Concatenate all text
        string concatenatedText = titleText + leadText + boxText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.Metropol,
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


internal class ArchivePaginator : SitemapArchivePaginator, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new ScrapingJobDescription
        {
            Url = new Uri(base.GetUrl(currentUrl, document)),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, doc));
    }
}

internal class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = GetLinks(docString).Select(link => new ScrapingJobDescription
        {
            Url = new Uri(link),
            Type = PageContentType.Article,
        })
                                        .ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = GetLinks(doc.ToString()).Select(link => new ScrapingJobDescription
        {
            Url = new Uri(link),
            Type = PageContentType.Article,
        }).ToList();
        return Task.FromResult(result);
    }
}
