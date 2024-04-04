using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class PestriSracokScraper : INewsSiteScraper
{
    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectNodes("//h1[contains(@class, 'story-title entry-title')]")[0];
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var articleNode = document.DocumentNode.SelectNodes("//div[contains(@class, 'wprt-container')]")[0];
        string articleText = articleNode.InnerText.Trim() + " ";


        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return Helper.CleanUpText(concatenatedText);
    }

    public Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        //TODO: ez itt elég trükkös lesz
        throw new NotImplementedException();
    }
}
