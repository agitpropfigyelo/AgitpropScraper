﻿using System.Globalization;
using System.Xml;
using Agitprop.Core;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;

namespace Agitprop.Scrapers.Ripost;

public class ArticleContentParser : IContentParser
{
    public (string, object) ParseContent(HtmlDocument html)
    {
        var titleNode = html.DocumentNode.SelectSingleNode("//h1[@class='title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = html.DocumentNode.SelectSingleNode("//div[@class='article-page-lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        var boxNodes = html.DocumentNode.SelectNodes("//app-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        string concatenatedText = titleText + leadText + boxText;

        return ("text", Helper.CleanUpText(concatenatedText));
    }

    public Task<(string, object)> ParseContentAsync(HtmlDocument html)
    {
        var result = ParseContent(html);
        return Task.FromResult(result);
    }

    public Task<(string, object)> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }
}

public class ArchivePaginator : SitemapArchivePaginator, IPaginator
{
    public ScrapingJob GetNextPage(string currentUrl, HtmlDocument document)
    {
        return new ScrapingJobBuilder().SetUrl(base.GetUrl(currentUrl, document))
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddContentParser(new ArticleContentParser())
                                       .AddPagination(new ArchivePaginator())
                                       .Build();
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        HtmlDocument html = new();
        html.LoadHtml(docString);
        return Task.FromResult(GetNextPage(currentUrl, html));

    }
}

public class ArchiveLinkParser : SitemapLinkParser, ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        var result = base.GetLinks(docString).Select(link => new ScrapingJobBuilder().SetUrl(link)
                                                                                     .SetPageType(PageType.Static)
                                                                                     .SetPageCategory(PageCategory.TargetPage)
                                                                                     .AddContentParser(new ArticleContentParser())
                                                                                     .Build()).ToList();
        return Task.FromResult(result);
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var result = base.GetLinks(doc.ToString()).Select(link => new ScrapingJobBuilder().SetUrl(link)
                                                                             .SetPageType(PageType.Static)
                                                                             .SetPageCategory(PageCategory.TargetPage)
                                                                             .AddContentParser(new ArticleContentParser())
                                                                             .Build()).ToList();
        return Task.FromResult(result);
    }
}
