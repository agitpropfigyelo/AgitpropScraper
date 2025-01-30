using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Agitprop.Scraper.Sinks.Newsfeed;

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
            logger.LogInformation($"Crawling started: {job.Url} ");
            var newJobs = await resiliencePipeline.ExecuteAsync(async ct => await spider.CrawlAsync(job, sink, ct));
            logger.LogInformation($"{job.Url} new jobs received: {newJobs.Count}");
            await context.PublishBatch(newJobs.Cast<List<NewsfeedJobDescrpition>>());

            logger.LogInformation($"Crawling finished {job.Url}");
        }
    }
}
