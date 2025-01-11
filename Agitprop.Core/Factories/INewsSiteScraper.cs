using HtmlAgilityPack;

namespace Agitprop.Core.Factories;

public interface INewsSiteScraper
{
    Task<List<string>> GetArticlesForDateAsync(DateTime dateIn);

    string GetArticleContent(HtmlDocument document);
}
