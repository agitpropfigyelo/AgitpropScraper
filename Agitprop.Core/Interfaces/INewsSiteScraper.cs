using HtmlAgilityPack;

namespace Agitprop.Core.Interfaces;

public interface INewsSiteScraper
{
    Task<List<string>> GetArticlesForDateAsync(DateTime dateIn);

    string GetArticleContent(HtmlDocument document);
}
