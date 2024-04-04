using System.Net;
using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class MagyarJelenScraper : INewsSiteScraper
{
    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='is-title post-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNode = document.DocumentNode.SelectSingleNode("//div[@class='post-content cf entry-content content-spacious']");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;


        return Helper.CleanUpText(concatenatedText);
    }

    public Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        throw new NotImplementedException();
    }
}
