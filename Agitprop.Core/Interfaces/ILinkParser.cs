using HtmlAgilityPack;

namespace Agitprop.Core.Interfaces;

public interface ILinkParser
{
    Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, string docString);
    Task<List<ScrapingJobDescription>> GetLinksAsync(string baseUrl, HtmlDocument doc)
;
}