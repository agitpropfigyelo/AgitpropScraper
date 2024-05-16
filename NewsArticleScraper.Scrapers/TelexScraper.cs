using HtmlAgilityPack;
using NewsArticleScraper.Core;

namespace NewsArticleScraper.Scrapers;

public class TelexScraper : INewsSiteScraper
{
    public string GetArticleContent(HtmlDocument document)
    {
        // Select nodes with class "article-title"
        var titleNode = document.DocumentNode.SelectSingleNode("//div[@class='title-section__top']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var articleNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'article-html-content')]");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        return Helper.CleanUpText(concatenatedText);
    }

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        int pageNum = 1;
        bool hasNextPage = true;
        List<string> result = [];
        while (hasNextPage)
        {
            try
            {
                Uri url = new($"https://telex.hu/legfrissebb?oldal={pageNum++}");
                using HttpClient client = new HttpClient();
                string htmlContent = await client.GetStringAsync(url);

                HtmlDocument doc = new();
                doc.LoadHtml(htmlContent);

                List<ArchiveArticleInfo> articles = GetArticles(doc.DocumentNode.SelectNodes("//div[@class='list__item__info']"));
                if (articles.First().PublishDate.Date < dateIn.Date) break;
                if (articles.Last().PublishDate.Date > dateIn.Date) continue;
                foreach (var article in articles)
                {
                    if (article.PublishDate.Date == dateIn.Date) result.Add(article.UrlToArticle);
                }
            }
            catch (System.Exception)
            {
                hasNextPage = false;
                throw;
            }
        }
        return result;
    }
    private List<ArchiveArticleInfo> GetArticles(HtmlNodeCollection articleCollectionIn)
    {
        List<ArchiveArticleInfo> result = [];
        foreach (HtmlNode? article in articleCollectionIn)
        {
            string link = article.FirstChild.GetAttributeValue("href", "");
            DateTimeOffset date =DateTimeOffset.Parse(string.Join(".",link.Split("/")[2..5]));
            result.Add(new ArchiveArticleInfo(link, date));
        }
        return result;
    }
}
