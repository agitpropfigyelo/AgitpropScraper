using Agitprop.Core;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Pestisracok;

public class ArchivePaginator : DateBasedArchive, IPaginator
{
    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        return Task.FromResult(new ScrapingJobBuilder().SetUrl(GetDateBasedUrl("https://www.pestisracok.hu", currentUrl))
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddPagination(new ArchivePaginator())
                                       .AddLinkParser(new ArchiveLinkParser())
                                       .Build());
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetNextPageAsync(currentUrl, doc);
    }
}
public class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = doc.DocumentNode.SelectNodes("//*[@id='home-widget-wrap']/div/ul/li/div[1]/a")
            .Select(x => x.GetAttributeValue("href", ""))
            .Select(link => new ScrapingJobBuilder().SetUrl(link)
                                                    .SetPageCategory(PageCategory.TargetPage)
                                                    .SetPageType(PageType.Static)
                                                    .AddContentParser(new ArticleContentParser())
                                                    .Build()).ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}

public class ArticleContentParser : IContentParser
{
    public Task<(string, object)> ParseContentAsync(HtmlDocument html)
    {
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectNodes("//h1[contains(@class, 'story-title entry-title')]")[0];
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNode = html.DocumentNode.SelectNodes("//div[contains(@class, 'wprt-container')]")[0];
        string articleText = articleNode.InnerText.Trim() + " ";


        // Concatenate all text
        string concatenatedText = titleText + articleText;

        (string, object) result = ("text", Helper.CleanUpText(concatenatedText));
        return Task.FromResult(result);
    }

    public Task<(string, object)> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }

}