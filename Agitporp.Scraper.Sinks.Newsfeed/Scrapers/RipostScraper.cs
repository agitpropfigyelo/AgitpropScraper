using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Ripost;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("/html/body/app-root/app-base/div/app-article-page/section/div[1]/div/app-article-page-head/div/div/div[2]/div[1]");
        DateTime date = DateTime.Parse(dateNode.InnerText.Split(':')[1]);

        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = html.DocumentNode.SelectSingleNode("//div[@class='article-page-lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        var boxNodes = html.DocumentNode.SelectNodes("//app-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        string concatenatedText = titleText + leadText + boxText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.Ripost,
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
    public NewsfeedJobDescrpition GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new NewsfeedJobDescrpition
        {
            Url = new Uri(base.GetUrl(currentUrl, document)),
            Type = PageContentType.Archive,
        };
    }

    public Task<NewsfeedJobDescrpition> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument html = new();
        html.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, html));

    }
}

internal class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<NewsfeedJobDescrpition>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = base.GetLinks(docString)
                         .Select(link => new NewsfeedJobDescrpition
                         {
                             Url = new Uri(link),
                             Type = PageContentType.Article,
                         })
                         .ToList();
        return Task.FromResult(result);
    }

    public Task<List<NewsfeedJobDescrpition>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = base.GetLinks(doc.ToString())
                         .Select(link => new NewsfeedJobDescrpition
                         {
                             Url = new Uri(link),
                             Type = PageContentType.Article,
                         })
                         .ToList();
        return Task.FromResult(result);
    }
}
