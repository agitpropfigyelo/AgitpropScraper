namespace Agitprop.Core.Interfaces;

/// <summary>
/// Defines the contract for paginating through web pages.
/// </summary>
public interface IPaginator
{
    /// <summary>
    /// Retrieves the next page based on the current URL and document content.
    /// </summary>
    /// <param name="currentUrl">The URL of the current page.</param>
    /// <param name="docString">The content of the current page as a string.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the description of the next page.</returns>
    public Task<ScrapingJobDescription> GetNextPageAsync(string currentUrl, string docString);
}
