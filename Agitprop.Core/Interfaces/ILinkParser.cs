using HtmlAgilityPack;

namespace Agitprop.Core.Interfaces;

/// <summary>
/// Defines the contract for parsing links from web pages.
/// </summary>
public interface ILinkParser
{
    /// <summary>
    /// Retrieves links from a web page based on its base URL and content as a string.
    /// </summary>
    /// <param name="baseUrl">The base URL of the web page.</param>
    /// <param name="docString">The content of the web page as a string.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of scraping job descriptions.</returns>
    Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString);

    /// <summary>
    /// Retrieves links from a web page based on its base URL and an HTML document.
    /// </summary>
    /// <param name="baseUrl">The base URL of the web page.</param>
    /// <param name="doc">The HTML document of the web page.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of scraping job descriptions.</returns>
    Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc);
}
