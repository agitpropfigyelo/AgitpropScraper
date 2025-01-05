using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public class Spider : ISpider
{
    private ILogger<Spider> Logger;
    private IBrowserPageLoader BrowserPageLoader;
    private IStaticPageLoader StaticPageLoader;
    private IConfiguration Configuration;

    public Spider(ILogger<Spider> logger, IBrowserPageLoader browserPageLoader, IStaticPageLoader staticPageLoader, IConfiguration configuration)
    {
        Logger = logger;
        BrowserPageLoader = browserPageLoader;
        StaticPageLoader = staticPageLoader;
        Configuration = configuration;
    }

    public async Task<List<ScrapingJobDescription>> CrawlAsync(ScrapingJob job, IEnumerable<ISink> sinks, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var htmlContent = job.PageType switch
        {
            PageType.Static => await LoadStaticPage(job),
            PageType.Dynamic => await LoadDynamicPage(job, Configuration.GetValue<bool>("Headless")),
            _ => throw new NotImplementedException()
        };

        HtmlDocument doc = new();
        doc.LoadHtml(htmlContent);

        if (job.PageCategory == PageCategory.TargetPage)
        {
            await ProcessTargetPage(job, doc, sinks, cancellationToken);

            //await LinkTracker.AddVisitedLinkAsync(job.Url);

            return Enumerable.Empty<ScrapingJobDescription>().ToList();
        }

        List<ScrapingJobDescription> newJobs = [];
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
        if (job.PageCategory == PageCategory.PageWithPagination && Configuration.GetValue<bool>("Continous"))
        {
            newJobs.Add(await job.Pagination!.GetNextPageAsync(job.Url, htmlContent));
        }
        if (job.PageCategory != PageCategory.PageWithPagination)
        {
            // await LinkTracker.AddVisitedLinkAsync(job.Url);
        }
        return newJobs;
    }

    private async Task ProcessTargetPage(ScrapingJob job, HtmlDocument doc, IEnumerable<ISink> sinks, CancellationToken cancellationToken = default)
    {
        List<ContentParserResult> results = [];
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

        if (results.Count == 0) throw new ContentParserException($"No content was scraped from: {job.Url}");

        Logger.LogInformation($"Sending scraped data to sinks {job.Url}...");
        foreach (var item in sinks)
        {
            item.Emit(job.Url, results, cancellationToken);
        }

        Logger.LogInformation($"Finished waiting for sinks {job.Url}");
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
}
