using NewsArticleScraper.Core;
using HtmlAgilityPack;

namespace NewsArticleScraper.Scrapers;

public class OrigoScraper : INewsSiteScraper
{
    private readonly Uri baseUri = new Uri("https://www.origo.hu");

    public string GetArticleContent(HtmlDocument document)
    {
        string text = document.DocumentNode.SelectSingleNode("/html/body/app-root/app-base/div[2]/app-article-page/section").InnerText;
        if (string.IsNullOrEmpty(text)) throw new InvalidOperationException("Not able to scrape site");
        return text.Replace("&nbsp;", "");
    }

    public async Task<List<string>> GetArticlesAsync(DateTime dateIn)
    {

        List<string> resultArticles = [];
        string archivePath = $"/hir-archivum/{dateIn.Year}/{dateIn:yyyyMMdd}.html";
        Uri url = new(baseUri, archivePath);

        try
        {
            using (HttpClient client = new HttpClient())
            {
                string htmlContent = await client.GetStringAsync(url);

                HtmlDocument doc = new();
                doc.LoadHtml(htmlContent);

                var hrefs = doc.DocumentNode.Descendants("article")
                    .Select(article => article.Descendants("a").FirstOrDefault())
                    .Where(a => a != null)
                    .Select(a => a.GetAttributeValue("href", ""))
                    .ToList();
                resultArticles = hrefs.Select(link => new Uri(baseUri, link).ToString()).ToList();
            }
        }
        catch (Exception ex)
        {
            // Rethrow the exception as a task result
            throw new InvalidOperationException("Error occurred while fetching articles", ex);
            //add logging
        }

        return resultArticles;
    }
}
