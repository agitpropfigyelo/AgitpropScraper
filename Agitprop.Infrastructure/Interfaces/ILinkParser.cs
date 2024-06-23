using HtmlAgilityPack;

namespace Agitprop.Infrastructure.Interfaces;

public interface ILinkParser
{
    Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, string docString);
    Task<List<ScrapingJob>> GetLinksAsync(string baseUrl, HtmlDocument doc)
;
}