using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class HuszonnegyScraper : INewsSiteScraper
{
    private readonly Uri baseUri = new Uri("https://www.24.hu");
    private readonly Uri sitemapBase = new Uri("https://24.hu/app/uploads/sitemap/");

    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        HtmlNode titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='o-post__title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var leadNode = document.DocumentNode.SelectSingleNode("//h1[@class='o-post__lead lead post-lead cf _ce_measure_widget']");
        string leadText = leadNode is not null ? leadNode.InnerText.Trim() + " " : "";

        // Select nodes with class "article-lead"
        var articleNode = document.DocumentNode.SelectSingleNode("//div[@class='o-post__body post-body']");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + leadText + articleText;


        return Helper.CleanUpText(concatenatedText);

    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        try
        {

            Uri url = new($"https://24.hu/{dateIn.Year}/{dateIn.Month}/{dateIn.Day}");
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(url);
                HtmlDocument doc = new();
                doc.LoadHtml(htmlContent);

                HtmlNodeCollection articles =doc.DocumentNode.SelectNodes("//*[@id='content']/h2/a");
                return articles.Select(x => x.GetAttributeValue("href","")).ToList();
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
