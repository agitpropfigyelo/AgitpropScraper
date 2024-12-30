using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.Huszonnegy;
internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = html.DocumentNode.SelectSingleNode("//*[@id='content']/div/div[1]/div[1]/div[5]/div[1]/div[2]/span");
        DateTime date = DateTime.Parse(dateNode.InnerText);
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='o-post__title']");
        var titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = html.DocumentNode.SelectSingleNode("//h1[@class='o-post__lead lead post-lead cf _ce_measure_widget']");
        var leadText = leadNode is not null ? leadNode.InnerText.Trim() + " " : "";

        // Select nodes with class "article-lead"
        var articleNode = html.DocumentNode.SelectSingleNode("//div[@class='o-post__body post-body']");
        var articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        var concatenatedText = titleText + leadText + articleText;
        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.Huszonnegy,
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
    public Task<List<NewsfeedJobDescrpition>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var nodes = doc.DocumentNode.SelectNodes("//*[@id='content']/h2/a");
        var result = nodes.Select(x => x.GetAttributeValue("href", ""))
                          .Select(url => new NewsfeedJobDescrpition
                          {
                              Url = new Uri(url),
                              Type = PageContentType.Article,
                          })
                          .ToList();
        return Task.FromResult(result);
    }
    public Task<List<NewsfeedJobDescrpition>> GetLinksAsync(string baseUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}


internal class ArchivePaginator : DateBasedArchive, IPaginator
{
    public async Task<NewsfeedJobDescrpition> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var url = GetDateBasedUrl("https://24.hu", currentUrl);
        var job = new NewsfeedJobDescrpition
        {
            Url = new Uri(url),
            Type = PageContentType.Archive,
        };
        return await Task.FromResult(job);
    }

    public async Task<NewsfeedJobDescrpition> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return await GetNextPageAsync(currentUrl, doc);
    }
}
