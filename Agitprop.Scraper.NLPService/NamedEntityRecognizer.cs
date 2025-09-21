using Polly;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Agitprop.Scraper.NLPService;

/// <summary>
/// Provides functionality for recognizing named entities in text using an external service, with retries and tracing.
/// </summary>
public class NamedEntityRecognizer : INamedEntityRecognizer
{
    private readonly HttpClient _client;
    private readonly ILogger<NamedEntityRecognizer> _logger;
    private readonly int _retryCount;
    private static readonly ActivitySource _activitySource = new("Agitprop.NamedEntityRecognizer");

    public NamedEntityRecognizer(HttpClient client, ILogger<NamedEntityRecognizer> logger, IConfiguration? configuration = null)
    {
        _client = client;
        _logger = logger;
        _retryCount = configuration?.GetValue<int>("Retry:NLPService", 3) ?? 3;
    }

    public async Task<string> PingAsync()
    {
        using var activity = _activitySource.StartActivity("PingNLPService", ActivityKind.Client);
        try
        {
            _logger.LogInformation("Pinging NLP service");
            var response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    _retryCount,
                    attempt => TimeSpan.FromSeconds(0.5 * attempt),
                    (outcome, ts, attempt, ctx) =>
                    {
                        if (outcome.Exception != null)
                            _logger.LogWarning(outcome.Exception, "[RETRY] Exception pinging NLP service on attempt {Attempt}", attempt);
                        else if (outcome.Result != null)
                            _logger.LogWarning("[RETRY] Failed to ping NLP service on attempt {Attempt}. Status: {StatusCode}", attempt, outcome.Result.StatusCode);
                    })
                .ExecuteAsync(() => _client.GetAsync("ping"));

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ping NLP service");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<NamedEntityCollection> AnalyzeSingleAsync(object corpus)
    {
        using var activity = _activitySource.StartActivity("AnalyzeSingleCorpus", ActivityKind.Client);
        activity?.SetTag("corpus.length", corpus?.ToString()?.Length ?? 0);

        _logger.LogInformation("Analyzing single corpus");

        var json = JsonSerializer.Serialize(corpus);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    _retryCount,
                    attempt => TimeSpan.FromSeconds(0.5 * attempt),
                    (outcome, ts, attempt, ctx) =>
                    {
                        if (outcome.Exception != null)
                            _logger.LogWarning(outcome.Exception, "[RETRY] Exception analyzing single corpus on attempt {Attempt}", attempt);
                        else if (outcome.Result != null)
                            _logger.LogWarning("[RETRY] Failed to analyze single corpus on attempt {Attempt}. Status: {StatusCode}", attempt, outcome.Result.StatusCode);
                    })
                .ExecuteAsync(() => _client.PostAsync("analyzeSingle", content));

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<NamedEntityCollection>(responseBody) ?? new NamedEntityCollection();
            _logger.LogInformation("Single corpus analyzed successfully. Entities found: {Count}", result.All.Count);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze single corpus");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<NamedEntityCollection[]> AnalyzeBatchAsync(object[] corpora)
    {
        using var activity = _activitySource.StartActivity("AnalyzeBatchCorpus", ActivityKind.Client);
        activity?.SetTag("batch.size", corpora?.Length ?? 0);

        _logger.LogInformation("Analyzing batch of {Count} corpora", corpora.Length);

        var json = JsonSerializer.Serialize(corpora);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    _retryCount,
                    attempt => TimeSpan.FromSeconds(0.5 * attempt),
                    (outcome, ts, attempt, ctx) =>
                    {
                        if (outcome.Exception != null)
                            _logger.LogWarning(outcome.Exception, "[RETRY] Exception analyzing batch corpus on attempt {Attempt}", attempt);
                        else if (outcome.Result != null)
                            _logger.LogWarning("[RETRY] Failed to analyze batch corpus on attempt {Attempt}. Status: {StatusCode}", attempt, outcome.Result.StatusCode);
                    })
                .ExecuteAsync(() => _client.PostAsync("analyzeBatch", content));

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<NamedEntityCollection[]>(responseBody) ?? Array.Empty<NamedEntityCollection>();
            _logger.LogInformation("Batch analysis completed successfully. Total corpora: {Count}", result.Length);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze batch corpus");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
