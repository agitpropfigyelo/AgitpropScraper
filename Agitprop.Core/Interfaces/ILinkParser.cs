using HtmlAgilityPack;

namespace Agitprop.Core.Interfaces;

public interface ILinkParser
{
    Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString);
    Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
;
}