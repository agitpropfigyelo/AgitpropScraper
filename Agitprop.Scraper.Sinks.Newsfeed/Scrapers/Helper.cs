using System.Net;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace Agitprop.Scraper.Sinks.Newsfeed.Scrapers;

internal static class Helper
{
    internal static string CleanUpText(string textIn)
    {
        // Decode HTML text
        var text = WebUtility.HtmlDecode(textIn);
        // Replace all new lines with space
        text = text.Replace("\n", " ").Replace("\r", " ");
        // Replace all tabs with a single space
        text = text.Replace("\t", " ");
        // Remove multiple consecutive spaces
        text = Regex.Replace(text, @"\s+", " ");
        // Trim leading and trailing spaces
        text = text.Trim();

        return text;
    }
}
