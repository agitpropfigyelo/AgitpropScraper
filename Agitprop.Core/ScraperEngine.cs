using System.ComponentModel;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using PuppeteerSharp;

namespace Agitprop.Core;

public class ScraperEngine : BackgroundService
{
    public ScraperEngine(ScraperConfig config, IScheduler scheduler, ISpider spider, ILogger<ScraperEngine> logger)
    {
        this.config = config;
        Scheduler = scheduler;
        Spider = spider;
        Logger = logger;
    }

    public ScraperConfig config { get; init; }
    public IScheduler Scheduler { get; init; }
    public ISpider Spider { get; init; }
    public ILogger<ScraperEngine> Logger { get; init; }
    public int ParallelismDegree { get; init; } = 4;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Scheduler.Initialization;


        //resiliency pipeline, találj neki jobb helyet VAGY TODO: rakd össze a DI-t az egész projektre
        var builder = new ResiliencePipelineBuilder();
        //builder.AddRetry(new CircuitBreaker)


        Logger.LogInformation("Start {class}.{method}", nameof(ScraperEngine), nameof(ExecuteAsync));

        foreach (var job in config.StartJobs)
        {
            Logger.LogInformation("Scheduling the initial scraping job with start url {startUrl}", job.Url);

            await Scheduler.AddAsync(job, cancellationToken);
        }

        var options = new ParallelOptions { MaxDegreeOfParallelism = ParallelismDegree };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Logger.LogInformation("Start consuming the scraping jobs");
            int idk = 0;

            await Parallel.ForEachAsync(Scheduler.GetAllAsync(cancellationToken), options, async (jobIn, token) =>
            {
                token.ThrowIfCancellationRequested();
                Console.WriteLine(idk);
                Interlocked.Increment(ref idk);
                Logger.LogDebug($"Running: {idk} - {jobIn}");
                if (jobIn is ScrapingJob job)
                {

                    Logger.LogInformation($"{job.Url} start crawling url ");
                    List<ScrapingJob> newJobs = [];

                    try
                    {
                        //   newJobs = await Executor.RetryAsync(() => Spider.CrawlAsync(job, cancellationToken));
                        newJobs = await Spider.CrawlAsync(job, cancellationToken);
                    }
                    catch (NavigationException ex)//ez
                    {
                        Logger.LogError(ex, $"Failed to scrape {job.Url}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.LogError(ex, $"Failed to scrape {job.Url}");
                    }
                    catch (HttpRequestException ex)//ez
                    {
                        Logger.LogError(ex, $"Failed to scrape {job.Url}");
                    }
                    catch (PageAlreadyVisitedException ex)
                    {
                        Logger.LogError(ex, $"Page has been already scraped {job.Url}");
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Add("url", job.Url);
                        throw;
                    }

                    Logger.LogInformation($"{job.Url} received {newJobs.Count} new jobs");
                    cancellationToken.ThrowIfCancellationRequested();

                    await Scheduler.AddAsync(newJobs, cancellationToken);
                    Logger.LogInformation($"{job.Url} finished {idk}");
                }
            });
            Logger.LogInformation("Finished jobs from scheduler");
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
        catch (OperationCanceledException ex)
        {
            Logger.LogWarning(ex, "Shutting down due tue cancelled operation");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Shutting down due to unhandled exception");
            throw;
        }
        finally
        {
            await Scheduler.Close();
        }
    }
}
