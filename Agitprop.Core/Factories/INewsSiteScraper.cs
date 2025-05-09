using HtmlAgilityPack;

namespace Agitprop.Core.Factories;

/// <summary>
/// Defines the contract for scraping news articles from a website.
/// </summary>
public interface INewsSiteScraper
{
    /// <summary>
    /// Retrieves a list of article URLs for a specific date.
    /// </summary>
    /// <param name="dateIn">The date for which to retrieve articles.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of article URLs.</returns>
    Task<List<string>> GetArticlesForDateAsync(DateTime dateIn);

    /// <summary>
    /// Extracts the content of an article from an HTML document.
    /// </summary>
    /// <param name="document">The HTML document containing the article.</param>
    /// <returns>The content of the article as a string.</returns>
    string GetArticleContent(HtmlDocument document);
}
