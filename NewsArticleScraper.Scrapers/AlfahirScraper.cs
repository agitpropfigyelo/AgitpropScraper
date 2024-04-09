using System.ComponentModel;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class AlfahirScraper : INewsSiteScraper
{
    private Uri baseUri = new("https://alfahir.hu/");
    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='article-title']");
        string titleText = titleNode.InnerText.Trim() + " ";

        // Select nodes with class "article-lead"
        var leadNode = document.DocumentNode.SelectSingleNode("//p[@class='article-lead']");
        string leadText = leadNode.InnerText.Trim() + " ";

        // Select nodes with tag "origo-wysiwyg-box"
        var articleNode = document.DocumentNode.SelectNodes("//div[contains(@class, 'article-content')]")[1];
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + leadText + articleText;

        return Helper.CleanUpText(concatenatedText);
    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        List<string> resultArticles = [];
        int pageNum = 1;
        bool moveToNextPage = true;

        try
        {
            while (moveToNextPage)
            {
                string archivePath = $"/hirek/oldalak/{pageNum++}";
                Uri url = new(baseUri, archivePath);
                using (HttpClient client = new HttpClient())
                {
                    string htmlContent = await client.GetStringAsync(url);

                    HtmlDocument doc = new();
                    doc.LoadHtml(htmlContent);

                    List<ArchiveArticleInfo> articleInfos = [];
                    HtmlNodeCollection articleNodes = doc.DocumentNode.SelectNodes(".//div[@class='article']");
                    foreach (var item in articleNodes)
                    {
                        articleInfos.Add(CreateArticleInfo(item));
                    }
                    resultArticles.AddRange(articleInfos.Where(info => info.PublishDate.Date == dateIn.Date).Select(info => info.UrlToArticle));
                    var min = articleInfos.Min(info => info.PublishDate.Date);
                    if (dateIn.Date > min.Date)
                    {
                        moveToNextPage = false;
                        break;
                    }


                }
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

    private ArchiveArticleInfo CreateArticleInfo(HtmlNode nodeIn)
    {
        HtmlNode idk = nodeIn.SelectSingleNode(".//a[@class='article-title-link']");
        var link = idk.GetAttributeValue<string>("href", "");
        var pubDate = DateTimeOffset.Parse(nodeIn.SelectSingleNode(".//span[@class='article-date']").InnerText);
        return new ArchiveArticleInfo(link,pubDate);
    }
}
