using Agitprop.Core;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public class ScraperEngine
{
    public IScraperConfigStore ConfigStorage { get; init; }
    public IScheduler Scheduler { get; init; }
    public ISpider Spider { get; init; }
    public ILogger Logger { get; init; }
    public int ParallelismDegree { get; init; }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await Scheduler.Initialization;

        Logger.LogInformation("Start {class}.{method}", nameof(ScraperEngine), nameof(RunAsync));

        ScraperConfig config = await ConfigStorage.GetConfigAsync();

        foreach (var job in config.StartJobs)
        {
            Logger.LogInformation("Scheduling the initial scraping job with start url {startUrl}", job.Url);

            await Scheduler.AddAsync(job, cancellationToken);
        }

        var options = new ParallelOptions { MaxDegreeOfParallelism = ParallelismDegree };

        try
        {
            Logger.LogInformation("Start consuming the scraping jobs");

            await Parallel.ForEachAsync(Scheduler.GetAllAsync(cancellationToken), options, async (jobIn, token) =>
            {
                if (jobIn is ScrapingJob job)
                {

                    Logger.LogInformation("Start crawling url {Url}", job.Url);

                    //var newJobs = await RetryAsync(async() => await Spider.CrawlAsync(job, cancellationToken));
                    List<ScrapingJob> newJobs = await Executor.RetryAsync(() => Spider.CrawlAsync(job, cancellationToken));

                    Logger.LogInformation("Received {JobsCount} new jobs", newJobs.Count);

                    await Scheduler.AddAsync(newJobs, cancellationToken);
                }
            });
        }
        catch (PageCrawlLimitException ex)
        {
            Logger.LogWarning(ex, "Shutting down due to page crawl limit {Limit}", ex.PageCrawlLimit);
        }
        catch (TaskCanceledException ex)
        {
            Logger.LogWarning(ex, "Shutting down due to cancellation");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Shutting down due to unhandled exception");
            throw;
        }
    }
}
