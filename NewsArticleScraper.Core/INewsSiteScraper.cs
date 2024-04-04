using HtmlAgilityPack;

namespace NewsArticleScraper.Core;

public interface INewsSiteScraper
{
    Task<List<string>> GetArticlesForDateAsync(DateTime dateIn);

    string GetArticleContent(HtmlDocument document);
}
