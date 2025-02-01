using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;

using HtmlAgilityPack;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public sealed class Spider(
    IBrowserPageLoader browserPageLoader,
    IStaticPageLoader staticPageLoader,
    IConfiguration configuration,
    ILogger<Spider>? logger = default) : ISpider
{
    private ILogger<Spider>? Logger = logger;
    private IBrowserPageLoader BrowserPageLoader = browserPageLoader;
    private IStaticPageLoader StaticPageLoader = staticPageLoader;
    private IConfiguration Configuration = configuration;

    public async Task<List<ScrapingJobDescription>> CrawlAsync(ScrapingJob job, ISink sink, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Check if the link is already visited
        if (await sink.CheckPageAlreadyVisited(job.Url))
        {
            Logger?.LogInformation($"Page already visited: {job.Url}");
            return Enumerable.Empty<ScrapingJobDescription>().ToList();
        }

        var doc = await LoadPageAsync(job);

        if (job.PageCategory == PageCategory.TargetPage)
        {
            await ProcessTargetPage(job, doc, sink, cancellationToken);
            return Enumerable.Empty<ScrapingJobDescription>().ToList();
        }

        List<ScrapingJobDescription> newJobs = [];
        foreach (var linkParser in job.LinkParsers)
        {
            try
            {
                newJobs.AddRange(await linkParser.GetLinksAsync(job.Url, doc.ToString() ?? throw new ArgumentException("Failed to get convert html doc to string")));
            }
            catch (Exception)
            {
                Logger?.LogWarning($"Failed to get links from site: {job.Url}");
            }
        }
        if (job.PageCategory == PageCategory.PageWithPagination && Configuration.GetValue<bool>("Continous"))
        {
            newJobs.Add(await job.Pagination!.GetNextPageAsync(job.Url, doc.ToString() ?? throw new ArgumentException("Failed to get convert html doc to string")));
        }

        return newJobs;
    }

    private async Task ProcessTargetPage(ScrapingJob job, HtmlDocument doc, ISink sink, CancellationToken cancellationToken = default)
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
                Logger?.LogWarning($"{job.Url} Failed to run content parser: {ex.Message}");
                throw;
            }
        }

        if (results.Count == 0) throw new ContentParserException($"No content was scraped from: {job.Url}");

        Logger?.LogInformation($"Sending scraped data to sink {job.Url}...");
        await sink.EmitAsync(job.Url, results, cancellationToken);

        Logger?.LogInformation($"Finished waiting for sink {job.Url}");
    }

    private async Task<HtmlDocument> LoadPageAsync(ScrapingJob job)
    {
        var htmlContent = job.PageType switch
        {
            PageType.Static => await LoadStaticPage(job),
            PageType.Dynamic => await LoadDynamicPage(job, Configuration.GetValue<bool>("Headless")),
            _ => throw new NotImplementedException()
        };

        HtmlDocument doc = new();
        doc.LoadHtml(htmlContent);
        return doc;
    }

    private async Task<string> LoadDynamicPage(ScrapingJob job, bool headless)
    {
        Logger?.LogInformation("{Url} Loading dynamic page", job.Url);
        var doc = await BrowserPageLoader.Load(job.Url, job?.Actions, headless);

        return doc;
    }

    private async Task<string> LoadStaticPage(ScrapingJob job)
    {
        Logger?.LogInformation("Loading static page {Url}", job.Url);
        var doc = await StaticPageLoader.Load(job.Url);

        return doc;
    }
}
