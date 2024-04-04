using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class RtlScraper : INewsSiteScraper
{
    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='page-layout__title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'static-page__content static-page__content--lead')]");
        string leadText = leadNode.InnerText.Trim() + " ";

        var articleNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'static-page__content static-page__content--body')]");
        string articleText = Helper.ConcatenateNodeText(articleNodes);

        // Concatenate all text
        string concatenatedText = titleText + leadText + articleText;

        return Helper.CleanUpText(concatenatedText);
    }

    public Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        throw new NotImplementedException();
    }
}
