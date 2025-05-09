using Agitprop.Core;

namespace Agitprop.Infrastructure.Interfaces;

/// <summary>
/// Defines the contract for a web scraping spider.
/// </summary>
public interface ISpider
{
    /// <summary>
    /// Crawls a web page based on the provided scraping job and sends the results to the specified sink.
    /// </summary>
    /// <param name="job">The scraping job containing details about the page to scrape.</param>
    /// <param name="sink">The sink to which the scraped data will be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of descriptions for new scraping jobs discovered during the crawl.</returns>
    Task<List<ScrapingJobDescription>> CrawlAsync(ScrapingJob job, ISink sink, CancellationToken cancellationToken = default);
}
