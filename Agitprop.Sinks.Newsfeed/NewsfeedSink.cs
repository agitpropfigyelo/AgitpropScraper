using Polly;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

using Agitprop.Core;
using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Sinks.Newsfeed;

public class NewsfeedSink : ISink
{
    private readonly INamedEntityRecognizer _nerService;
    private readonly INewsfeedDB _db;
    private readonly ILogger<NewsfeedSink> _logger;
    private readonly ActivitySource _activitySource = new("Agitprop.NewsfeedSink");
    private readonly int _retryCount;

    public NewsfeedSink(INamedEntityRecognizer nerService, INewsfeedDB db, ILogger<NewsfeedSink> logger, IConfiguration? configuration = null)
    {
        _nerService = nerService;
        _db = db;
        _logger = logger;
        _retryCount = configuration?.GetValue<int>("Retry:NewsfeedSink", 3) ?? 3;
    }

    public async Task<bool> CheckPageAlreadyVisited(string url)
    {
        using var trace = _activitySource.StartActivity("CheckPageAlreadyVisited", ActivityKind.Internal);
        try
        {
            if (_logger != null) _logger.LogInformation("Checking if page already visited: {url}", url);

            var exists = await Polly.Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (ex, ts, attempt, ctx) =>
                {
                    _logger?.LogWarning(ex, "[RETRY] Exception checking page {url} on attempt {attempt}", url, attempt);
                })
                .ExecuteAsync(() => _db.IsUrlAlreadyExists(url));

            _logger?.LogInformation("CheckPageAlreadyVisited result for {url}: {exists}", url, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to check if page already visited: {url}", url);
            trace?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task EmitAsync(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default)
    {
        using var trace = _activitySource.StartActivity("EmitAsync", ActivityKind.Internal);
        _logger?.LogInformation("Processing {articleCount} articles for {url}", data.Count, url);

        foreach (var article in data)
        {
            trace?.SetTag("articleLength", article.Text.Length);

            try
            {
                var entities = await Polly.Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (ex, ts, attempt, ctx) =>
                    {
                        _logger?.LogWarning(ex, "[RETRY] Exception analyzing entities for {url} attempt {attempt}", url, attempt);
                    })
                    .ExecuteAsync(() => _nerService.AnalyzeSingleAsync(article.Text));

                _logger?.LogInformation("Received {entityCount} entities for article in {url}", entities.All.Count, url);

                var count = await Polly.Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (ex, ts, attempt, ctx) =>
                    {
                        _logger?.LogWarning(ex, "[RETRY] Exception inserting mentions for {url} attempt {attempt}", url, attempt);
                    })
                    .ExecuteAsync(() => _db.CreateMentionsAsync(url, article, entities));

                _logger?.LogInformation("Inserted {count} mentions for article in {url}", count, url);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to process article for {url}", url);
                trace?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }

        _logger?.LogInformation("Finished processing articles for {url}", url);
    }
}
