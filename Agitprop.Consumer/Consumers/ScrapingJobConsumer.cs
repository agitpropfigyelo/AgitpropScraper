namespace Agitprop.Consumer.Consumers
{
    using System.Threading.Tasks;
    using MassTransit;
    using Agitprop.Core.Enums;
    using System;
    using Agitprop.Infrastructure.Interfaces;
    using Agitprop.Core;
    using Agitprop.Core.Interfaces;
    using Agitprop.Scrapers.Factories;
    using Polly.Registry;
    using Microsoft.Extensions.Logging;
    using System.Net.Http;
    using PuppeteerSharp;
    using Agitprop.Core.Exceptions;
    using Agitprop.Core.Contracts;


    public class ScrapingJobConsumer :
        IConsumer<ScrapingJobDescription>
    {
        private ISpider spider;
        private IProgressReporter progressReporter;
        private ScrapingJobFactory factory;
        private ILogger<ScrapingJobConsumer> logger;
        ResiliencePipelineProvider<string> ResiliencePipelineProvider;

        public ScrapingJobConsumer(ISpider spider, ScrapingJobFactory factory, ILogger<ScrapingJobConsumer> logger, ResiliencePipelineProvider<string> resiliencePipelineProvider, IProgressReporter progressReporter = default)
        {
            this.spider = spider;
            this.progressReporter = progressReporter;
            this.factory = factory;
            this.logger = logger;
            ResiliencePipelineProvider = resiliencePipelineProvider;

        }

        public async Task Consume(ConsumeContext<ScrapingJobDescription> context)
        {

            ScrapingJobDescription descriptor = context.Message;
            var source = descriptor.Url.Host;
            var job = CreateJob(descriptor);

            var pipeline = ResiliencePipelineProvider.GetPipeline("Spider");
            try
            {
                logger.LogInformation($"Crawling started: {job.Url} ");
                var newJobs = await pipeline.ExecuteAsync(async ct => await spider.CrawlAsync(job, progressReporter, ct));
                logger.LogInformation($"{job.Url} new jobs received: {newJobs.Count}");
                await context.PublishBatch(newJobs);
                progressReporter?.ReportNewJobsScheduled(newJobs.Count);

                logger.LogInformation($"Crawling finished {job.Url}");
                progressReporter?.ReportJobSuccess(job.Url);
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
                progressReporter?.ReportJobFailed(job.Url);
            }
            catch (PageAlreadyVisitedException ex)
            {
                progressReporter?.ReportJobSkipped(job.Url);
                logger.LogError(ex, $"Failed to scrape {job.Url}");
            }
        }

        private NewsSites GetNewssiteFromUrl(Uri url)
        {
            return url.Host.ToLower() switch
            {
                "www.origo.hu" => NewsSites.Origo,
                "ripost.hu" => NewsSites.Ripost,
                "mandiner.hu" => NewsSites.Mandiner,
                "metropol.hu" => NewsSites.Metropol,
                "magyarnemzet.hu" => NewsSites.MagyarNemzet,
                "pestisracok.hu" => NewsSites.PestiSracok,
                "magyarjelen.hu" => NewsSites.MagyarJelen,
                "kurucz.info" => NewsSites.Kuruczinfo,
                "alfahir.hu" => NewsSites.Alfahir,
                "24.hu" => NewsSites.Huszonnegy,
                "444.hu" => NewsSites.NegyNegyNegy,
                "hvg.hu" => NewsSites.HVG,
                "telex.hu" => NewsSites.Telex,
                "rtl.hu" => NewsSites.RTL,
                "index.hu" => NewsSites.Index,
                "merce.hu" => NewsSites.Merce,
                _ => throw new ArgumentException($"Not supported news source: {url.Host}")
            };

        }
        private ScrapingJob CreateJob(ScrapingJobDescription jobDescription)
        {
            var site = GetNewssiteFromUrl(jobDescription.Url);
            switch (jobDescription.Type)
            {
                case PageContentType.Archive:
                    return factory.GetArchiveScrapingJob(site, jobDescription.Url.ToString());
                case PageContentType.Article:
                    return factory.GetArticleScrapingJob(site, jobDescription.Url.ToString());
                default:
                    throw new ArgumentException($"Content type \"{jobDescription.Type}\" is not supported {jobDescription.Url}");

            }
        }
    }
}