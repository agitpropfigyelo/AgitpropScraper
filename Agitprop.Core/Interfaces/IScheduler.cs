using Agitprop.Core;

namespace Agitprop.Core.Interfaces;

public interface IScheduler
{
    Task Initialization { get; }

    Task AddAsync(IEnumerable<ScrapingJob> newJobs, CancellationToken cancellationToken);
    Task AddAsync(ScrapingJob newJobs, CancellationToken cancellationToken);
    IAsyncEnumerable<ScrapingJob> GetAllAsync(CancellationToken cancellationToken);
    Task Close();
}
