using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Agitprop.Scraper.Sinks.Newsfeed;
using Agitprop.Infrastructure.Interfaces;

using MassTransit;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Registry;

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
            logger.LogInformation("Crawling started: {url}", job.Url);
            List<Core.ScrapingJobDescription> newJobs = await resiliencePipeline.ExecuteAsync(async ct => await spider.CrawlAsync(job, sink, ct));
            logger.LogInformation("New jobs {count} received from: {url}", newJobs.Count, job.Url);
            List<NewsfeedJobDescrpition> idk =newJobs.Select(x=>(NewsfeedJobDescrpition)x).ToList();
            await context.PublishBatch(idk);

            logger.LogInformation("Crawling finished: {url}", job.Url);
        }
    }
}
