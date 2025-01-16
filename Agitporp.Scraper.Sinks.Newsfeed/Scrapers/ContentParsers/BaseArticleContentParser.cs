using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitporp.Scraper.Sinks.Newsfeed.Scrapers.ContentParsers;

internal abstract class BaseArticleContentParser : IContentParser
{
    protected abstract List<string> DateXPaths { get; }
    protected abstract List<string> TitleXPaths { get; }
    protected abstract List<string> LeadXPaths { get; }
    protected abstract List<string> ArticleXPaths { get; }
    protected abstract NewsSites SourceSite { get; }

    private HtmlNode SelectSingleNode(HtmlDocument doc, List<string> xpaths)
    {
        foreach (var xpath in xpaths)
        {
            var node = doc.DocumentNode.SelectSingleNode(xpath);
            if (node != null)
            {
                return node;
            }
        }
        return null;
    }

    private List<HtmlNode> SelectMultipleNodes(HtmlDocument doc, List<string> xpaths)
    {
        var nodes = new List<HtmlNode>();
        foreach (var xpath in xpaths)
        {
            var selectedNodes = doc.DocumentNode.SelectNodes(xpath);
            if (selectedNodes != null)
            {
                nodes.AddRange(selectedNodes);
            }
        }
        return nodes;
    }

    public Task<ContentParserResult> ParseContentAsync(HtmlDocument html)
    {
        var dateNode = SelectSingleNode(html, DateXPaths);
        DateTime date = DateTime.Parse(dateNode.InnerText);

        var titleNode = SelectSingleNode(html, TitleXPaths);
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = SelectSingleNode(html, LeadXPaths);
        string leadText = leadNode != null ? leadNode.InnerText.Trim() + " " : "";

        var articleNodes = SelectMultipleNodes(html, ArticleXPaths);
        string articleText = string.Join(" ", articleNodes.Select(node => node.InnerText.Trim()));

        string concatenatedText = titleText + leadText + articleText;

        return Task.FromResult(new ContentParserResult()
        {
            PublishDate = date,
            SourceSite = SourceSite,
            Text = Helper.CleanUpText(concatenatedText)
        });
    }

    public Task<ContentParserResult> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return ParseContentAsync(doc);
    }
}
