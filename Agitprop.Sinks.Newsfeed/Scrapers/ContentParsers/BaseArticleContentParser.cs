using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;

using HtmlAgilityPack;

namespace Agitprop.Sinks.Newsfeed.Scrapers.ContentParsers;

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
        try
        {

            var dateNode = SelectSingleNode(html, DateXPaths);
            DateTime date = DateTime.Parse(dateNode.Attributes["content"].Value);
            if (date == DateTime.MinValue) throw new ContentParserException("Date not found");

            var titleNode = SelectSingleNode(html, TitleXPaths);
            string titleText = titleNode.InnerText.Trim() + " ";

            var leadNode = SelectSingleNode(html, LeadXPaths);
            string leadText = leadNode != null ? leadNode.InnerText.Trim() + " " : "";

            var articleNodes = SelectMultipleNodes(html, ArticleXPaths);
            string articleText = string.Join(" ", articleNodes.Select(node => node.InnerText.Trim()));

            string concatenatedText = titleText + leadText + articleText;
            if (string.IsNullOrWhiteSpace(concatenatedText))
            {
                throw new ContentParserException("Article's content not found");
            }

            return Task.FromResult(new ContentParserResult()
            {
                Title = Helper.CleanUpText(titleText.Trim()),
                PublishDate = date,
                SourceSite = SourceSite,
                Text = Helper.CleanUpText(concatenatedText)
            });
        }
        catch (NullReferenceException ex)
        {
            throw new ContentParserException("Failed to scrape page", ex);
        }
    }

    public Task<ContentParserResult> ParseContentAsync(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        return ParseContentAsync(doc);
    }
}
