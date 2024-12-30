using System.Threading.Tasks;
using MassTransit;
using Agitprop.Core.Enums;
using System;
using Agitprop.Infrastructure.Interfaces;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Polly.Registry;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using PuppeteerSharp;
using Agitprop.Core.Exceptions;
using Polly;
using Agitporp.Scraper.Sinks.Newsfeed;
using System.Linq;
using System.Collections.Generic;

namespace Agitprop.Consumer.Consumers
{
    public class NewsfeedJobConsumer :
        IConsumer<NewsfeedJobDescrpition>
    {
        private ISpider spider;
        private ILogger<NewsfeedJobConsumer> logger;
        private ResiliencePipeline resiliencePipeline;

        public NewsfeedJobConsumer(ISpider spider, ILogger<NewsfeedJobConsumer> logger, ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            this.spider = spider;
            this.logger = logger;
            resiliencePipeline = resiliencePipelineProvider.GetPipeline("Spider");

        }

        public async Task Consume(ConsumeContext<NewsfeedJobDescrpition> context)
        {

            NewsfeedJobDescrpition descriptor = context.Message;
            var job = descriptor.ConvertToScrapingJob();
            try
            {
                logger.LogInformation($"Crawling started: {job.Url} ");
                var newJobs = await resiliencePipeline.ExecuteAsync(async ct => await spider.CrawlAsync(job, ct));
                logger.LogInformation($"{job.Url} new jobs received: {newJobs.Count}");
                await context.PublishBatch(newJobs.Cast<List<NewsfeedJobDescrpition>>());

                logger.LogInformation($"Crawling finished {job.Url}");
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
                logger.LogError(ex, $"Failed to scrape {job.Url}");
            }
            catch (PageAlreadyVisitedException ex)
            {
                logger.LogError(ex, $"Failed to scrape {job.Url}");
            }
        }
    }
}