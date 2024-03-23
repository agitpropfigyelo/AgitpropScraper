using HtmlAgilityPack;

namespace NewsArticleScraper.Core;

public interface INewsSiteScraper
{
    Task<List<string>> GetArticlesAsync(DateTime dateIn);

    string GetArticleContent(HtmlDocument document);
}
