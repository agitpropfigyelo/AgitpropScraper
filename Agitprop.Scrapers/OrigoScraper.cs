using NewsArticleScraper.Core;
using HtmlAgilityPack;
using System.Text;
using System.Net;

namespace NewsArticleScraper.Scrapers;

public class OrigoScraper : INewsSiteScraper
{
    private readonly Uri baseUri = new Uri("https://www.origo.hu");

    public string GetArticleContent(HtmlDocument document)
    {
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='article-title']");
        string titleText = titleNode.InnerText.Trim()+" ";

        var leadNode = document.DocumentNode.SelectSingleNode("//div[@class='article-lead']");
        string leadText = leadNode.InnerText.Trim()+" ";

        var boxNodes = document.DocumentNode.SelectNodes("//origo-wysiwyg-box");
        string boxText = Helper.ConcatenateNodeText(boxNodes);

        string concatenatedText = titleText + leadText + boxText;

        return Helper.CleanUpText(concatenatedText);
    }


    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
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
                //TODO: use XPATH like other scrapers
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
