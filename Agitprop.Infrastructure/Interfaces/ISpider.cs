namespace Agitprop.Infrastructure.Interfaces;

public interface ISpider
{
    Task<List<ScrapingJob>> CrawlAsync(ScrapingJob job, CancellationToken cancellationToken);
}
