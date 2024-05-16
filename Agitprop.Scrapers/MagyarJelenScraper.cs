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

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {

        int pageNum = 1;
        List<string> result = [];
        bool isArchiveEnded = false;
        while (!isArchiveEnded)
        {
            try
            {

                Uri url = new($"https://magyarjelen.hu/{dateIn.Year}/{dateIn.Month}/{dateIn.Day}/page/{pageNum++}/");
                using (HttpClient client = new HttpClient())
                {
                    string htmlContent = await client.GetStringAsync(url);
                    HtmlDocument doc = new();
                    doc.LoadHtml(htmlContent);
                    HtmlNodeCollection articles = doc.DocumentNode.SelectNodes("//div[@class='col-8 main-content']/section/div/div/article/div[1]/a");
                    result.AddRange(articles.Select(x => x.GetAttributeValue("href", "")).ToList());
                }
            }
            catch (HttpRequestException ex)
            {
                isArchiveEnded = true;
            }
            catch (Exception ex)
            {
                // Rethrow the exception as a task result
                throw new InvalidOperationException("Error occurred while fetching articles", ex);
                //add logging
            }
        }
        return result;
    }
}
