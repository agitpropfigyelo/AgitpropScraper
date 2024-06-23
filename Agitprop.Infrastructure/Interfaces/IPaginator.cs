using HtmlAgilityPack;

namespace Agitprop.Infrastructure;

public interface IPaginator
{
    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString);
}
