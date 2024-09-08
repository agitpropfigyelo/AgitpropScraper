using Agitprop.Core.Enums;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Agitprop.Core;

public class Spider : ISpider
{
    private List<ISink> Sinks;
    private ILogger Logger;
    private ILinkTracker LinkTracker;
    private IStaticPageLoader StaticPageLoader;
    private IBrowserPageLoader BrowserPageLoader;
    private IScraperConfigStore ScraperConfigStore;

    public Spider(List<ISink> sinks, ILinkTracker linkTracker, IStaticPageLoader staticPageLoader,
                  IBrowserPageLoader browserPageLoader, IScraperConfigStore scraperConfigStore, ILogger logger)
    {
        Sinks = sinks;
        LinkTracker = linkTracker;
        StaticPageLoader = staticPageLoader;
        BrowserPageLoader = browserPageLoader;
        ScraperConfigStore = scraperConfigStore;
        Logger = logger;
    }

    public async Task<List<ScrapingJob>> CrawlAsync(ScrapingJob job, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await LinkTracker.Initialization;

        var config = await ScraperConfigStore.GetConfigAsync();

        if (config.UrlBlackList.Contains(job.Url)) return Enumerable.Empty<ScrapingJob>().ToList();

        await CheckCrawlLimit(config);
        if (job.PageCategory != PageCategory.PageWithPagination) await LinkTracker.AddVisitedLinkAsync(job.Url); ;
        var htmlContent = job.PageType switch
        {
            PageType.Static => await LoadStaticPage(job),
            PageType.Dynamic => await LoadDynamicPage(job, config.Headless),
            _ => throw new NotImplementedException()
        };

        HtmlDocument doc = new();
        doc.LoadHtml(htmlContent);

        if (job.PageCategory == PageCategory.TargetPage)
        {
            await ProcessTargetPage(job, doc, cancellationToken);

            await CheckCrawlLimit(config);

            return Enumerable.Empty<ScrapingJob>().ToList();
        }

        List<ScrapingJob> newJobs = [];
        foreach (var linkParser in job.LinkParsers)
        {
            try
            {
                newJobs.AddRange(await linkParser.GetLinksAsync(job.Url, htmlContent));
            }
            catch (Exception)
            {
                Logger.LogWarning($"Failed to get links from site: {job.Url}");
            }
        }
        if (job.PageCategory == PageCategory.PageWithPagination)
        {
            newJobs.Add(await job.Pagination!.GetNextPageAsync(job.Url, htmlContent));
        }
        return newJobs;
    }

    private async Task ProcessTargetPage(ScrapingJob job, HtmlDocument doc, CancellationToken cancellationToken = default)
    {
        List<ContentParserResult> results = [];
        //await job.ContentParsers.Select(async parser => await parser.ParseContentAsync(doc)).ToDictionary();
        foreach (var contentParser in job.ContentParsers)
        {
            try
            {
                var idk = await contentParser.ParseContentAsync(doc);
                results.Add(idk);

            }
            catch (Exception ex)
            {
                Logger.LogWarning($"{job.Url} Failed to run content parser: {ex.Message}");
            }
        }

        Logger.LogInformation("Sending scraped data to sinks...");
        var sinkTasks = Sinks.Select(sink => sink.EmitAsync(job.Url, results, cancellationToken));

        Logger.LogInformation("Waiting for sinks ...");
        await Task.WhenAll(sinkTasks);
        Logger.LogInformation("Finished waiting for sinks");
    }

    private async Task<string> LoadDynamicPage(ScrapingJob job, bool headless)
    {
        Logger.LogInformation("{Url} Loading dynamic page", job.Url);
        var doc = await BrowserPageLoader.Load(job.Url, job.Actions, headless);

        return doc;
    }

    private async Task<string> LoadStaticPage(ScrapingJob job)
    {
        Logger.LogInformation("Loading static page {Url}", job.Url);
        var doc = await StaticPageLoader.Load(job.Url);

        return doc;
    }

    private async Task CheckCrawlLimit(ScraperConfig config)
    {
        if (await LinkTracker.GetVisitedLinksCount() >= config.PageCrawlLimit)
        {
            Logger.LogInformation("Page crawl limit has been reached");

            throw new PageCrawlLimitException("Page crawl limit has been reached.")
            {
                PageCrawlLimit = config.PageCrawlLimit
            };
        }
    }

}
