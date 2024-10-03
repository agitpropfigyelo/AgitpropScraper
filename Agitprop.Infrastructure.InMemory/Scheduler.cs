using System.Threading.Channels;
using Agitprop.Core;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure.InMemory;

public class Scheduler : IScheduler
{
    private readonly Channel<ScrapingJob> _jobChannel = Channel.CreateUnbounded<ScrapingJob>();

    public IAsyncEnumerable<ScrapingJob> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _jobChannel.Reader.ReadAllAsync(cancellationToken);
    }

    public bool DataCleanupOnStart { get; set; }

    public Task Initialization { get; } = Task.CompletedTask;

    public async Task AddAsync(ScrapingJob job, CancellationToken cancellationToken = default)
    {
        await _jobChannel.Writer.WriteAsync(job, cancellationToken);
    }

    public async Task AddAsync(IEnumerable<ScrapingJob> jobs, CancellationToken cancellationToken = default)
    {
        foreach (var job in jobs)
            await _jobChannel.Writer.WriteAsync(job, cancellationToken);
    }

    public Task Close()
    {
        _jobChannel.Writer.Complete();
        return Task.CompletedTask;
    }
}