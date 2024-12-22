using System.Net;
using Agitprop.Core;
using Agitprop.Core.Contracts;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Kurucinfo;

internal class ArticleContentParser : IContentParser
{
    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        //TODO: a weboldal iso-8859-2 encoding-al van, valahogy ki kéne kupálni, hogy jó legyen
        //Convert this mofo to utf8, like all other normal newssite is, also fucked up using of html encoding
        //document= document.LoadHtml();

        var dateNode = html.DocumentNode.SelectSingleNode("//*[@id='cikkcontent']/div/p[1]/span[1]");
        DateTime date = DateTime.Parse(dateNode.InnerText);

        // Select nodes with class "article-title"       
        var titleNode = html.DocumentNode.SelectSingleNode("//div[@class='focikkheader']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNodes = html.DocumentNode.SelectNodes("//div[contains(@class, 'cikktext')]");
        string articleText = Helper.ConcatenateNodeText(articleNodes);


        // Concatenate all text
        string concatenatedText = WebUtility.HtmlDecode(titleText + articleText);

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = NewsSites.Kurucinfo,
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
    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var jobs = doc.DocumentNode.SelectNodes(".//div[@class='alcikkheader']/a")
                                   .Select(x => x.GetAttributeValue<string>("href", ""))
                                   .Select(url => new ScrapingJobDescription
                                   {
                                       Url = new Uri(url),
                                       Type = PageContentType.Article,
                                   })
                                   .ToList();
        return Task.FromResult(jobs);
    }

    public Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}

public class ArchivePaginator : IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        int pageNum = int.Parse(currentUrl.Split("/")[^2]) + 20;
        var result = new ScrapingJobDescription
        {
            Url = new Uri($"https://kuruc.info/to/1/{pageNum}/"),
            Type = PageContentType.Archive,
        };
        return Task.FromResult(result);
    }

    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetNextPageAsync(currentUrl, doc);
    }
}