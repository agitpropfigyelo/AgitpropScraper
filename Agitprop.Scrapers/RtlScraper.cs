using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Rtl;

public class ArchivePaginator : IPaginator
{
    public ScrapingJob GetNextPage(string currentUrl, HtmlDocument document)
    {
        var url = new Uri(currentUrl);
        var newUlr = $"https://rtl.hu/legfrissebb?oldal=1";
        if (int.TryParse(url.Query.Split('=')[1], out var page))
        {
            newUlr = $"https://rtl.hu/legfrissebb?oldal={++page}";
        }
        return new ScrapingJobBuilder().SetUrl(newUlr)
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddPagination(new ArchivePaginator())
                                       .AddLinkParser(new ArchiveLinkParser())
                                       .Build();
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return Task.FromResult(this.GetNextPage(currentUrl, doc));
    }
}

public class ArchiveLinkParser : ILinkParser
{
    private readonly Uri baseUri = new Uri("https://www.rtl.hu");

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var jobs = doc.DocumentNode.SelectNodes("//article").Select(x => x.FirstChild.GetAttributeValue("href", ""))
                                   .Select(link => new ScrapingJobBuilder().SetUrl(new Uri(baseUri, link).ToString())
                                                                           .SetPageCategory(PageCategory.TargetPage)
                                                                           .SetPageType(PageType.Static)
                                                                           .AddContentParser(new ArticleContentParser())
                                                                           .Build())
                                   .ToList();

        return Task.FromResult(jobs);
    }
}

public class ArticleContentParser : IContentParser
{
    public ContentParserResult ParseContent(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("//*[@id='app']/main/section/div/div[2]/div[1]/div[2]/p");
        DateTime date = DateTime.Parse(dateNode.InnerText);

        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='page-layout__title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'static-page__content static-page__content--lead')]");
        string leadText = leadNode.InnerText.Trim() + " ";

        var articleNodes = html.DocumentNode.SelectNodes("//div[contains(@class, 'static-page__content static-page__content--body')]");
        string articleText = Helper.ConcatenateNodeText(articleNodes);

        // Concatenate all text
        string concatenatedText = titleText + leadText + articleText;

        return new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.RTL,
            Text = Helper.CleanUpText(concatenatedText)
        };
    }

    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        return Task.FromResult(this.ParseContent(html));
    }
    public Task<ContentParserResult> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }
}
