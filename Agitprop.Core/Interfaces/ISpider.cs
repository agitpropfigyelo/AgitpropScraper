using Agitprop.Core;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure.Interfaces;

public interface ISpider
{
    Task<List<ScrapingJob>> CrawlAsync(ScrapingJob job, IProgressReporter progressReporter, CancellationToken cancellationToken);
}
