using System.Diagnostics;

using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;

using HtmlAgilityPack;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

/// <summary>
/// Represents a web scraping spider that can crawl web pages and process their content.
/// </summary>
public sealed class Spider(
    IBrowserPageLoader browserPageLoader,
    IStaticPageLoader staticPageLoader,
    IConfiguration configuration,
    ILogger<Spider>? logger = default) : ISpider
{
    // Logger instance for logging information and warnings.
    private ILogger<Spider>? Logger = logger;

    // Loader for dynamic web pages.
    private IBrowserPageLoader BrowserPageLoader = browserPageLoader;

    // Loader for static web pages.
    private IStaticPageLoader StaticPageLoader = staticPageLoader;

    // Configuration settings for the spider.
    private IConfiguration Configuration = configuration;

    // Activity source for tracing operations.
    private ActivitySource ActivitySource = new("Agitprop.Spider");

    /// <summary>
    /// Crawls a web page based on the provided scraping job and sends the results to the specified sink.
    /// </summary>
    /// <param name="job">The scraping job containing details about the page to scrape.</param>
    /// <param name="sink">The sink to which the scraped data will be sent.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of descriptions for new scraping jobs discovered during the crawl.</returns>
    public async Task<List<ScrapingJobDescription>> CrawlAsync(ScrapingJob job, ISink sink, CancellationToken cancellationToken = default)
    {
        this.ActivitySource.StartActivity("CrawlAsync", ActivityKind.Internal, job.Url);
        cancellationToken.ThrowIfCancellationRequested();

        // Check if the link is already visited
        if (await sink.CheckPageAlreadyVisited(job.Url))
        {
            Logger?.LogInformation("Page already visited: {url}", job.Url);
            return Enumerable.Empty<ScrapingJobDescription>().ToList();
        }

        var doc = await LoadPageAsync(job);

        if (job.PageCategory == PageCategory.TargetPage)
        {
            Logger?.LogInformation("Processing: {url}", job.Url);
            await ProcessTargetPage(job, doc, sink, cancellationToken);
            return Enumerable.Empty<ScrapingJobDescription>().ToList();
        }

        List<ScrapingJobDescription> newJobs = [];
        foreach (var linkParser in job.LinkParsers)
        {
            try
            {
                newJobs.AddRange(await linkParser.GetLinksAsync(job.Url, doc ?? throw new ArgumentException("Failed to convert HTML document to string")));
            }
            catch (Exception)
            {
                Logger?.LogWarning("Failed to get links from site: {url}", job.Url);
            }
        }
        if (job.PageCategory == PageCategory.PageWithPagination && Configuration.GetValue<bool>("Continous"))
        {
            newJobs.Add(await job.Pagination!.GetNextPageAsync(job.Url, doc.ToString() ?? throw new ArgumentException("Failed to convert HTML document to string")));
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
                Logger?.LogWarning("Failed to run content parser on {url}: {msg}",job.Url,ex.Message);
                throw new Exception($"Failed to run content parsing {job.Url}", ex);
            }
        }

        if (results.Count == 0) throw new ContentParserException($"No content was scraped from: {job.Url}");

        Logger?.LogInformation("Sending scraped data to sink: {url} ", job.Url);
        await sink.EmitAsync(job.Url, results, cancellationToken);

        Logger?.LogInformation("Finished waiting for sink: {url} ", job.Url);
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
        Logger?.LogInformation("Loading dynamic page {url}", job.Url);
        var doc = await BrowserPageLoader.Load(job.Url, job?.Actions, headless);

        return doc;
    }

    private async Task<string> LoadStaticPage(ScrapingJob job)
    {
        Logger?.LogInformation("Loading static page {url}", job.Url);
        var doc = await StaticPageLoader.Load(job.Url);

        return doc;
    }
}
