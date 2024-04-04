using System.Net;
using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class MagyarNemzetScraper : INewsSiteScraper
{
    public string GetArticleContent(HtmlDocument document)
    {
                // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='title']");
        string titleText = titleNode.InnerText.Trim()+" ";

        // Select nodes with class "article-lead"
        var leadNode = document.DocumentNode.SelectSingleNode("//h2[@class='lead']");
        string leadText = leadNode.InnerText.Trim()+" ";

        // Select nodes with tag "origo-wysiwyg-box"
        var boxNodes = document.DocumentNode.SelectNodes("//app-article-text");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        // Concatenate all text
        string concatenatedText = titleText + leadText + boxText;

        return Helper.CleanUpText(concatenatedText);
    }

    public Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        throw new NotImplementedException();
    }
}
