using Agitprop.Core.Contracts;

namespace Agitprop.Core.Interfaces;

public interface IPaginator
{
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString);
}
