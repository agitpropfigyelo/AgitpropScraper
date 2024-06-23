namespace Agitprop.Scrapers.Telex;

using Agitprop.Core;
using Agitprop.Infrastructure;
using Agitprop.Infrastructure.Enums;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;

public class ArticleContentParser : IContentParser
{
    public Task<(string, object)> ParseContentAsync(HtmlDocument html)
    {
        // Select nodes with class "article-title"
        var titleNode = html.DocumentNode.SelectSingleNode("//div[@class='title-section__top']");
        string titleText = titleNode.InnerText.Trim() + " ";

        var articleNode = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'article-html-content')]");
        string articleText = articleNode.InnerText.Trim() + " ";

        // Concatenate all text
        string concatenatedText = titleText + articleText;

        (string, object) result = ("text", Helper.CleanUpText(concatenatedText));
        return Task.FromResult(result);
    }

    public Task<(string, object)> ParseContentAsync(string html)
    {
        var doc =new HtmlDocument();
        doc.LoadHtml(html);
        return this.ParseContentAsync(doc);
    }
}

public class ArchiveLinkParser : ILinkParser
{
    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
    {
        var articles = doc.DocumentNode.SelectNodes("//div[@class='list__item__info']").Select(x => x.FirstChild.GetAttributeValue("href", ""));
        return Task.FromResult(articles.Select(url => new ScrapingJobBuilder().SetUrl(url)
                                                                              .SetPageCategory(PageCategory.TargetPage)
                                                                              .SetPageType(PageType.Static)
                                                                              .AddContentParser(new ArticleContentParser())
                                                                              .Build()).ToList());
    }

    public Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return this.GetLinksAsync(baseUrl, doc);
    }
}
public class ArchivePaginator : IPaginator
{
    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, HtmlDocument document)
    {
        var url = new Uri(currentUrl);
        var newUlr = $"https://telex.hu/legfrissebb?oldal=1";
        if (int.TryParse(url.Query.Split('=')[1], out var page))
        {
            newUlr = $"https://telex.hu/legfrissebb?oldal={++page}";
        }
        return Task.FromResult(new ScrapingJobBuilder().SetUrl(newUlr)
                                       .SetPageType(PageType.Static)
                                       .SetPageCategory(PageCategory.PageWithPagination)
                                       .AddPagination(new ArchivePaginator())
                                       .AddLinkParser(new ArchiveLinkParser())
                                       .Build());
    }

    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(docString);
        return this.GetNextPageAsync(currentUrl, doc);
    }
}

public class TelexScraper
{

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
            catch (Exception)
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
            DateTimeOffset date = DateTimeOffset.Parse(string.Join(".", link.Split("/")[2..5]));
            result.Add(new ArchiveArticleInfo(link, date));
        }
        return result;
    }
}
