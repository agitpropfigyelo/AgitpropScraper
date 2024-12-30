using HtmlAgilityPack;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Core;
using Agitprop.Core.Contracts;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Origo;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("/html/body/app-root/app-base/div[2]/app-article-page/section/div[1]/div/app-article-header/article/app-article-meta/div/div[1]/div")
                    ?? html.DocumentNode.SelectSingleNode("/html/body/app-root/app-base/div[2]/app-sport-article-page/section/div/div[1]/div/app-sport-article-header/article/app-sport-article-author/div/div/div/div");
        DateTime date = DateTime.Parse(dateNode.InnerText);

        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='article-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = html.DocumentNode.SelectSingleNode("//div[@class='article-lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        var boxNodes = html.DocumentNode.SelectNodes("//origo-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        string concatenatedText = titleText + leadText + boxText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.Origo,
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
internal class ArchiveLinkParser : ILinkParser
{
    private readonly Uri baseUri = new Uri("https://www.origo.hu");

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);

    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var hrefs = doc.DocumentNode.Descendants("article")
                   .Select(article => article.Descendants("a").FirstOrDefault())
                   .Where(a => a != null)
                   .Select(a => a.GetAttributeValue("href", ""))
                   .ToList();
        var result = hrefs.Select(link => new Uri(baseUri, link).ToString())
                          .Select(link => new ScrapingJobDescription
                          {
                              Url = new Uri(link),
                              Type = PageContentType.Article,
                          })
                          .ToList();
        return Task.FromResult(result);
    }
}

internal class ArchivePaginator : DateBasedArchive, IPaginator
{
    public ScrapingJobDescription GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new ScrapingJobDescription
        {
            Url = new Uri(GetDateBasedUrl("https://www.origo.hu/hirarchivum", currentUrl)),
            Type = PageContentType.Archive,
        };
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
    protected static new string GetDateBasedUrl(string urlBase, string current)
    {
        var currentUrl = new Uri(current);
        var nextDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var toParse = currentUrl.Segments[2..5].ToList();

        if (DateOnly.TryParse(string.Join(".", toParse.Select(x => x.Replace("/", ""))), out DateOnly date))
        {
            nextDate = date.AddDays(-1);
        }
        return $"{urlBase}/{nextDate.Year:D4}/{nextDate.Month:D2}/{nextDate.Day:D2}";
    }
}
