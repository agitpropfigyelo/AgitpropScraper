namespace Agitprop.Scrapers.Telex;

using Agitprop.Core;
using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

public class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("//*[@id='cikk-content']/div[1]/div[2]/div[2]/p/span");
        DateTime date = DateTime.Parse(dateNode.InnerText.Split('–')[0]);

        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//div[@class='title-section__top']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var articleNode = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'article-html-content')]");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.Telex,
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

public class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var articles = doc.DocumentNode.SelectNodes("//div[@class='list__item__info']").Select(x => x.FirstChild.GetAttributeValue("href", ""));
        return Task.FromResult(articles.Select(url => new ScrapingJobDescription
        {
            Url = new Uri(url),
            Type = PageContentType.Article,
            Sinks = { }
        }).ToList());
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}
public class ArchivePaginator : IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var url = new Uri(currentUrl);
        var newUlr = $"https://telex.hu/legfrissebb?oldal=1";
        if (int.TryParse(url.Query.Split('=')[1], out var page))
        {
            newUlr = $"https://telex.hu/legfrissebb?oldal={++page}";
        }
        return Task.FromResult(new ScrapingJobDescription
        {
            Url = new Uri(newUlr),
            Type = PageContentType.Archive,
            Sinks = { }
        });
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return this.GetNextPageAsync(currentUrl, doc);
    }
}