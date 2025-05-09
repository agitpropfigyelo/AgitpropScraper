using HtmlAgilityPack;

namespace Agitprop.Core.Interfaces;

/// <summary>
/// Defines the contract for parsing content from web pages.
/// </summary>
public interface IContentParser
{
    /// <summary>
    /// Parses the content of a web page from an HTML document.
    /// </summary>
    /// <param name="html">The HTML document to parse.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the parsed content result.</returns>
    Task<ContentParserResult> ParseContentAsync(HtmlDocument html);

    /// <summary>
    /// Parses the content of a web page from an HTML string.
    /// </summary>
    /// <param name="html">The HTML string to parse.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the parsed content result.</returns>
    Task<ContentParserResult> ParseContentAsync(string html);
}
