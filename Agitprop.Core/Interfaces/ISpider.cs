using Agitprop.Core;

namespace Agitprop.Infrastructure.Interfaces;

public interface ISpider
{
    Task<List<ScrapingJobDescription>> CrawlAsync(ScrapingJob job, IEnumerable<ISink> sinks, CancellationToken cancellationToken = default);
}
