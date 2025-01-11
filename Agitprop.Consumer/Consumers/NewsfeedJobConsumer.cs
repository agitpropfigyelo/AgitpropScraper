using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Agitporp.Scraper.Sinks.Newsfeed;

using Agitprop.Core;
using Agitprop.Core.Exceptions;
using Agitprop.Infrastructure.Interfaces;

using MassTransit;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Registry;

using PuppeteerSharp;

namespace Agitprop.Consumer.Consumers
{
    public class NewsfeedJobConsumer :
        IConsumer<NewsfeedJobDescrpition>
    {
        private ISpider spider;
        private ILogger<NewsfeedJobConsumer> logger;
        private ResiliencePipeline resiliencePipeline;
        private NewsfeedSink sink;

        public NewsfeedJobConsumer(ISpider spider, ILogger<NewsfeedJobConsumer> logger, ResiliencePipelineProvider<string> resiliencePipelineProvider, NewsfeedSink sink)
        {
            this.spider = spider;
            this.logger = logger;
            resiliencePipeline = resiliencePipelineProvider.GetPipeline("Spider");
            this.sink = sink;
        }

        public async Task Consume(ConsumeContext<NewsfeedJobDescrpition> context)
        {

            NewsfeedJobDescrpition descriptor = context.Message;
            var job = descriptor.ConvertToScrapingJob();
            try
            {
                logger.LogInformation($"Crawling started: {job.Url} ");
                var newJobs = await resiliencePipeline.ExecuteAsync(async ct => await spider.CrawlAsync(job, sink, ct));
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
