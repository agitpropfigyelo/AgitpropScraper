using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class MerceScraper : INewsSiteScraper
{
    private readonly Uri baseUrl= new("https://merce.hu/");
    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='entry-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var articleNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'entry-content')]");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return Helper.CleanUpText(concatenatedText);
    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {

        try
        {

            Uri url = new(baseUrl,$"{dateIn.Year}/{dateIn.Month}/{dateIn.Day}/");
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(url);
                HtmlDocument doc = new();
                doc.LoadHtml(htmlContent);
                HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//article/a");
                return articles.Select(x => x.GetAttributeValue("href", "")).ToList();
            }
        }
        catch (Exception ex)
        {
            // Rethrow the exception as a task result
            throw new InvalidOperationException("Error occurred while fetching articles", ex);
            //add logging
        }
    }

}

