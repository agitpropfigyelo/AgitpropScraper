using Polly;
using System.Diagnostics;
using Agitprop.Core;
using Agitprop.Core.Enums;
using Agitprop.Core.Exceptions;
using Agitprop.Core.Interfaces;
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
    private readonly ILogger<Spider>? _logger = logger;
    private readonly IBrowserPageLoader _browserPageLoader = browserPageLoader;
    private readonly IStaticPageLoader _staticPageLoader = staticPageLoader;
    private readonly IConfiguration _configuration = configuration;
    private readonly ActivitySource _activitySource = new("Agitprop.Spider");

    public async Task<List<ScrapingJobDescription>> CrawlAsync(ScrapingJob job, ISink sink, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CrawlAsync", ActivityKind.Internal);
        activity?.SetTag("url", job.Url);
        cancellationToken.ThrowIfCancellationRequested();

        // Check if already visited
        if (await sink.CheckPageAlreadyVisited(job.Url))
        {
            _logger?.LogInformation("Page already visited: {Url}", job.Url);
            activity?.SetStatus(ActivityStatusCode.Ok, "Already visited");
            return new List<ScrapingJobDescription>();
        }

        var retryCount = _configuration.GetValue<int>("Retry:Spider", 3);

        HtmlDocument doc;
        try
        {
            doc = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt),
                    (ex, ts, attempt, ctx) => _logger?.LogWarning(ex, "[RETRY] Failed to load page {Url} on attempt {Attempt}", job.Url, attempt))
                .ExecuteAsync(() => LoadPageAsync(job));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load page after {RetryCount} attempts: {Url}", retryCount, job.Url);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        if (job.PageCategory == PageCategory.TargetPage)
        {
            _logger?.LogInformation("Processing target page: {Url}", job.Url);
            await ProcessTargetPage(job, doc, sink, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return new List<ScrapingJobDescription>();
        }

        List<ScrapingJobDescription> newJobs = new();
        foreach (var parser in job.LinkParsers)
        {
            try
            {
                var links = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt),
                        (ex, ts, attempt, ctx) => _logger?.LogWarning(ex, "[RETRY] Failed to get links from {Url} on attempt {Attempt}", job.Url, attempt))
                    .ExecuteAsync(() => parser.GetLinksAsync(job.Url, doc));
                newJobs.AddRange(links);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get links from site: {Url}", job.Url);
            }
        }

        if (job.PageCategory == PageCategory.PageWithPagination && _configuration.GetValue<bool>("Continous"))
        {
            try
            {
                var nextPage = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt),
                        (ex, ts, attempt, ctx) => _logger?.LogWarning(ex, "[RETRY] Failed to get next page for {Url} on attempt {Attempt}", job.Url, attempt))
                    .ExecuteAsync(() => job.Pagination!.GetNextPageAsync(job.Url, doc.ToString()));
                newJobs.Add(nextPage);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get next page for site: {Url}", job.Url);
            }
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        return newJobs;
    }

    private async Task ProcessTargetPage(ScrapingJob job, HtmlDocument doc, ISink sink, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("ProcessTargetPage", ActivityKind.Internal);
        activity?.SetTag("url", job.Url);

        var retryCount = _configuration.GetValue<int>("Retry:Spider", 3);
        List<ContentParserResult> results = new();

        foreach (var parser in job.ContentParsers)
        {
            try
            {
                var parsed = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt),
                        (ex, ts, attempt, ctx) => _logger?.LogWarning(ex, "[RETRY] Failed content parser on {Url}, attempt {Attempt}", job.Url, attempt))
                    .ExecuteAsync(() => parser.ParseContentAsync(doc));

                results.Add(parsed);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to run content parser on {Url}", job.Url);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw new ContentParserException($"Failed content parsing for {job.Url}", ex);
            }
        }

        if (!results.Any())
        {
            _logger?.LogError("No content scraped from: {Url}", job.Url);
            activity?.SetStatus(ActivityStatusCode.Error, "No content scraped");
            throw new ContentParserException($"No content scraped from: {job.Url}");
        }

        _logger?.LogInformation("Sending scraped data to sink: {Url}", job.Url);
        await sink.EmitAsync(job.Url, results, cancellationToken);
        _logger?.LogInformation("Finished processing target page: {Url}", job.Url);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    private async Task<HtmlDocument> LoadPageAsync(ScrapingJob job)
    {
        string htmlContent = job.PageType switch
        {
            PageType.Static => await LoadStaticPage(job),
            PageType.Dynamic => await LoadDynamicPage(job, _configuration.GetValue<bool>("Headless")),
            _ => throw new NotImplementedException()
        };

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        return doc;
    }

    private async Task<string> LoadDynamicPage(ScrapingJob job, bool headless)
    {
        _logger?.LogInformation("Loading dynamic page: {Url}", job.Url);
        return await _browserPageLoader.Load(job.Url, job.Actions, headless);
    }

    private async Task<string> LoadStaticPage(ScrapingJob job)
    {
        _logger?.LogInformation("Loading static page: {Url}", job.Url);
        return await _staticPageLoader.Load(job.Url);
    }
}
