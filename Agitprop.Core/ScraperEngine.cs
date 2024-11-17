using System.ComponentModel;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using PuppeteerSharp;

namespace Agitprop.Core;

public class ScraperEngine : BackgroundService
{

    public ScraperConfig config;
    public IScheduler Scheduler;
    public ISpider Spider;
    public ILogger<ScraperEngine> Logger;
    private IFailedJobLogger FailedJobLogger;
    private IProgressReporter progressReporter;
    ResiliencePipelineProvider<string> ResiliencePipelineProvider;

    public ScraperEngine(ScraperConfig config, IScheduler scheduler, ISpider spider, ILogger<ScraperEngine> logger, ResiliencePipelineProvider<string> resiliencePipelineProvider, IFailedJobLogger failedJobLogger, IProgressReporter progressReporter)
    {
        this.config = config;
        Scheduler = scheduler;
        Spider = spider;
        Logger = logger;
        ResiliencePipelineProvider = resiliencePipelineProvider;
        FailedJobLogger = failedJobLogger;
        this.progressReporter = progressReporter;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Scheduler.Initialization;
        var pipeline = ResiliencePipelineProvider.GetPipeline("Spider");
        List<ScrapingJob> failedJobs = [];

        Logger.LogInformation("Start {class}.{method}", nameof(ScraperEngine), nameof(ExecuteAsync));

        foreach (var job in config.StartJobs)
        {
            Logger.LogInformation("Scheduling the initial scraping job with start url {startUrl}", job.Url);

            await Scheduler.AddAsync(job, cancellationToken);
        }

        var options = new ParallelOptions { MaxDegreeOfParallelism = config.Parallelism };

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Logger.LogInformation("Start consuming the scraping jobs");
            int idk = 0;

            await Parallel.ForEachAsync(Scheduler.GetAllAsync(cancellationToken), options, async (jobIn, token) =>
            {
                token.ThrowIfCancellationRequested();
                Interlocked.Increment(ref idk);
                if (jobIn is ScrapingJob job)
                {

                    Logger.LogInformation($"Crawling started: {job.Url} ");
                    progressReporter.ReportJobStarted(jobIn.Url);
                    List<ScrapingJob> newJobs = [];

                    try
                    {
                        newJobs = await pipeline.ExecuteAsync(async ct => await Spider.CrawlAsync(jobIn, progressReporter, ct));
                    }
                    catch (Exception ex) when (
                        ex is HttpRequestException ||
                        ex is TaskCanceledException ||
                        ex is TimeoutException ||
                        ex is NavigationException ||
                        ex is InvalidOperationException ||
                        ex is ContentParserException ||
                        ex is OperationCanceledException
                        )
                    {
                        failedJobs.Add(jobIn);
                        Logger.LogError(ex, $"Failed to scrape {job.Url}");
                        progressReporter.ReportJobFailed(jobIn.Url);
                        await FailedJobLogger.LogFailedJobUrlAsync(jobIn.Url);
                    }
                    catch (PageAlreadyVisitedException ex)
                    {
                        progressReporter.ReportJobSkipped(jobIn.Url);
                        Logger.LogError(ex, $"Failed to scrape {job.Url}");
                    }
                    catch (Exception ex)
                    {
                        failedJobs.Add(jobIn);
                        ex.Data.Add("url", job.Url);
                        progressReporter.ReportJobFailed(jobIn.Url);
                        await FailedJobLogger.LogFailedJobUrlAsync(jobIn.Url);
                        throw;
                    }

                    Logger.LogInformation($"{job.Url} new jobs received: {newJobs.Count}");
                    await Scheduler.AddAsync(newJobs, cancellationToken);
                    progressReporter.ReportNewJobsScheduled(newJobs.Count);

                    Logger.LogInformation($"Crawling finished {jobIn.Url}");
                    progressReporter.ReportJobSuccess(job.Url);
                }
            });
            Logger.LogInformation("Finished jobs from scheduler");
        }
        catch (PageCrawlLimitException ex)
        {
            Logger.LogWarning(ex, "Shutting down due to page crawl limit {Limit}", ex.PageCrawlLimit);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Shutting down due to unhandled exception");
        }
        finally
        {
            await Scheduler.Close();
        }
    }
}
