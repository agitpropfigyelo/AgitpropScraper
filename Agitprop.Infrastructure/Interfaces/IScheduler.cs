namespace Agitprop.Infrastructure;

internal interface IScheduler
{
    Task Initialization { get; }

    Task AddAsync(IEnumerable<ScrapingJob> newJobs, CancellationToken cancellationToken);
    Task AddAsync(ScrapingJob newJobs, CancellationToken cancellationToken);
    IEnumerable<object> GetAllAsync(CancellationToken cancellationToken);
}
