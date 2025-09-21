using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agitprop.Core.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System.Diagnostics;
using Agitprop.Sinks.Newsfeed;
using System;

namespace Agitprop.Scraper.Consumer.Consumers
{
    /// <summary>
    /// Consumes newsfeed job descriptions and processes them using a web scraping spider.
    /// </summary>
    public class NewsfeedJobConsumer : IConsumer<NewsfeedJobDescrpition>
    {
        private readonly ISpider _spider;
        private readonly ILogger<NewsfeedJobConsumer> _logger;
        private readonly ResiliencePipeline _resiliencePipeline;
        private readonly NewsfeedSink _sink;
        private static readonly ActivitySource _activitySource = new("Agitprop.NewsfeedJobConsumer");

        public NewsfeedJobConsumer(
            ISpider spider,
            ILogger<NewsfeedJobConsumer> logger,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            NewsfeedSink sink)
        {
            _spider = spider;
            _logger = logger;
            _resiliencePipeline = resiliencePipelineProvider.GetPipeline("Spider");
            _sink = sink;
        }

        public async Task Consume(ConsumeContext<NewsfeedJobDescrpition> context)
        {
            using var activity = _activitySource.StartActivity("ConsumeNewsfeedJob", ActivityKind.Consumer);
            var descriptor = context.Message;
            activity?.SetTag("job.url", descriptor.Url);
            activity?.SetTag("job.type", descriptor.Type.ToString());

            _logger.LogInformation("Crawling started for URL: {Url}", descriptor.Url);

            try
            {
                var job = descriptor.ConvertToScrapingJob();

                // Create a span for the actual crawling
                using var crawlActivity = _activitySource.StartActivity("SpiderCrawl", ActivityKind.Internal);
                crawlActivity?.SetTag("crawl.url", job.Url);

                List<Core.ScrapingJobDescription> newJobs = await _resiliencePipeline.ExecuteAsync(
                    async ct => await _spider.CrawlAsync(job, _sink, ct));

                crawlActivity?.SetTag("newJobs.count", newJobs.Count);
                _logger.LogInformation("Crawling finished for URL: {Url}, new jobs found: {Count}", job.Url, newJobs.Count);

                if (newJobs.Count > 0)
                {
                    // Create a span for publishing the new jobs
                    using var publishActivity = _activitySource.StartActivity("PublishNewJobs", ActivityKind.Producer);
                    var idk = newJobs.Select(x => (NewsfeedJobDescrpition)x).ToList();
                    publishActivity?.SetTag("publish.jobs.count", idk.Count);
                    await context.PublishBatch(idk);
                    _logger.LogInformation("Published {Count} new jobs from URL: {Url}", idk.Count, job.Url);
                    publishActivity?.SetStatus(ActivityStatusCode.Ok);
                }

                crawlActivity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetStatus(ActivityStatusCode.Ok, "Job processed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing newsfeed job for URL: {Url}", descriptor.Url);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }
    }
}
