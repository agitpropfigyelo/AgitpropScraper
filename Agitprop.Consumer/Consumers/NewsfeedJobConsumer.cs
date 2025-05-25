using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agitprop.Infrastructure.Interfaces;

using MassTransit;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Registry;
using System.Diagnostics;

namespace Agitprop.Consumer.Consumers
{
    /// <summary>
    /// Consumes newsfeed job descriptions and processes them using a web scraping spider.
    /// </summary>
    public class NewsfeedJobConsumer :
        IConsumer<NewsfeedJobDescrpition>
    {
        private ISpider spider;
        private ILogger<NewsfeedJobConsumer> logger;
        private ResiliencePipeline resiliencePipeline;
        private NewsfeedSink sink;
        private ActivitySource ActivitySource = new("Agitprop.NewsfeedJobConsumer");

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsfeedJobConsumer"/> class.
        /// </summary>
        /// <param name="spider">The web scraping spider to use for crawling.</param>
        /// <param name="logger">The logger for logging information and errors.</param>
        /// <param name="resiliencePipelineProvider">The resilience pipeline provider for handling transient errors.</param>
        /// <param name="sink">The sink for processing scraped data.</param>
        public NewsfeedJobConsumer(ISpider spider, ILogger<NewsfeedJobConsumer> logger, ResiliencePipelineProvider<string> resiliencePipelineProvider, NewsfeedSink sink)
        {
            this.spider = spider;
            this.logger = logger;
            resiliencePipeline = resiliencePipelineProvider.GetPipeline("Spider");
            this.sink = sink;
        }

        /// <summary>
        /// Consumes a newsfeed job description and processes it.
        /// </summary>
        /// <param name="context">The consume context containing the job description.</param>
        public async Task Consume(ConsumeContext<NewsfeedJobDescrpition> context)
        {
            using var trace = this.ActivitySource.StartActivity("Consume");
            NewsfeedJobDescrpition descriptor = context.Message;
            var job = descriptor.ConvertToScrapingJob();
            logger.LogInformation("Crawling started: {url}", job.Url);
            List<Core.ScrapingJobDescription> newJobs = await resiliencePipeline.ExecuteAsync(async ct => await spider.CrawlAsync(job, sink, ct));
            logger.LogInformation("New jobs {count} received from: {url}", newJobs.Count, job.Url);
            List<NewsfeedJobDescrpition> idk = newJobs.Select(x => (NewsfeedJobDescrpition)x).ToList();
            await context.PublishBatch(idk);

            logger.LogInformation("Crawling finished: {url}", job.Url);
        }
    }
}
