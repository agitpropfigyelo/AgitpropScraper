using Agitprop.Core;

namespace Agitprop.Infrastructure.Interfaces;

public interface ISpider
{
    Task<List<ScrapingJobDescription>> CrawlAsync(ScrapingJob job, ISink sink, CancellationToken cancellationToken = default);
}
