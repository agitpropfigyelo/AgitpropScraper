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

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        try
        {

            Uri url = new($"https://pestisracok.hu/{dateIn.Year}/{dateIn.Month}/{dateIn.Day}");
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(url);
                HtmlDocument doc = new();
                doc.LoadHtml(htmlContent);

                HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//*[@id='home-widget-wrap']/div/ul/li/div[1]/a");
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
