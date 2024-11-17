using Agitprop.Core;
using Agitprop.Core.Contracts;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure.Interfaces;

public interface ISpider
{
    Task<List<ScrapingJobDescription>> CrawlAsync(ScrapingJob job, IProgressReporter progressReporter, CancellationToken cancellationToken = default);
}
