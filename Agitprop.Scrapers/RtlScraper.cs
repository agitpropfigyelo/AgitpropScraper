using System.Globalization;
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

    public async Task<List<string>> GetArticlesForDateAsync(DateTime dateIn)
    {
        int pageNum = 1;
        bool hasNextPage = true;
        List<string> result = [];
        while (hasNextPage)
        {
            try
            {
                Uri url = new($"https://rtl.hu/legfrissebb?oldal={pageNum++}");
                using var client = new HttpClient();
                string htmlContent = await client.GetStringAsync(url);

                HtmlDocument doc = new();
                doc.LoadHtml(htmlContent);

                var articles = GetArticles(doc.DocumentNode.SelectNodes("//article"));
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
            var link = article.FirstChild.GetAttributeValue("href", "");
            var dateText = article.SelectSingleNode("./a/div[2]/span").InnerText;
            var date = DateTimeOffset.Parse(dateText);
            result.Add(new ArchiveArticleInfo(link, date));
        }
        return result;
    }
}
