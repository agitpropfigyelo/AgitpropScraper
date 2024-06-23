﻿using System.Net;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Kuruczinfo;


public class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var jobs = doc.DocumentNode.SelectNodes(".//div[@class='alcikkheader']/a").Select(x => x.GetAttributeValue<string>("href", "")).Select(url =>
        {
            return new ScrapingJobBuilder().SetUrl(url)
                                           .SetPageCategory(PageCategory.TargetPage)
                                           .SetPageType(PageType.Static)
                                           .AddContentParser(new ArticleContentParser())
                                           .Build();
        }).ToList();
        return Task.FromResult(jobs);
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
        //TODO: a weboldal iso-8859-2 encoding-al van, valahogy ki kéne kupálni, hogy jó legyen
        //Convert this mofo to utf8, like all other normal newssite is, also fucked up using of html encoding
        //document= document.LoadHtml();

        // Select nodes with class "article-title"       
        var titleNode = html.DocumentNode.SelectSingleNode("//div[@class='focikkheader']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNodes = html.DocumentNode.SelectNodes("//div[contains(@class, 'cikktext')]");
        string articleText = Helper.ConcatenateNodeText(articleNodes);


        // Concatenate all text
        string concatenatedText = WebUtility.HtmlDecode(titleText + articleText);

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

public class ArchivePaginator : IPaginator
{
    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        int pageNum = int.Parse(currentUrl.Split("/")[^2]) + 20;
        var result = new ScrapingJobBuilder().SetUrl($"https://kuruc.info/to/1/{pageNum}/")
                                             .SetPageType(PageType.Static)
                                             .SetPageCategory(PageCategory.PageWithPagination)
                                             .AddPagination(new ArchivePaginator())
                                             .AddLinkParser(new ArchiveLinkParser())
                                             .Build();
        return Task.FromResult(result);
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(docString);
        return this.GetNextPageAsync(currentUrl, doc);
    }
}