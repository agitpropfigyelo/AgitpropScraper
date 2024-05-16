using System.Net;
using HtmlAgilityPack;

namespace NewsArticleScraper.Scrapers;

internal static class Helper
{
    internal static string ConcatenateNodeText(HtmlNodeCollection nodes)
    {
        if (nodes == null || nodes.Count == 0)
            return "";

        string concatenatedText = "";
        foreach (var node in nodes)
        {
            concatenatedText += node.InnerText.Trim() + " ";
        }
        return concatenatedText;
    }

    internal static string CleanUpText(string textIn)
    {
        var textToClean=WebUtility.HtmlDecode(textIn);
        return WebUtility.HtmlDecode(textIn)
                         .Replace("\n", " ")
                         .Replace("\t", "")
                         .Replace("\r", "");
    }
}
