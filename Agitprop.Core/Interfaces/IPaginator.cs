namespace Agitprop.Core.Interfaces;

public interface IPaginator
{
    public Task<ScrapingJob> GetNextPageAsync(string currentUrl, string docString);
}
