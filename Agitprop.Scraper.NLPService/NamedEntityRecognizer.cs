using Polly;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

using Agitprop.Core;
using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Scraper.NLPService;

/// <summary>
/// Provides functionality for recognizing named entities in text using an external service.
/// </summary>
public class NamedEntityRecognizer : INamedEntityRecognizer
{
    private readonly HttpClient _client;
    private readonly ILogger<NamedEntityRecognizer> Logger;
    private readonly int _retryCount;

    public NamedEntityRecognizer(HttpClient client, ILogger<NamedEntityRecognizer> logger, IConfiguration? configuration = null)
    {
        _client = client;
        Logger = logger;
    _retryCount = configuration?.GetValue<int>("Retry:NLPService", 3) ?? 3;
    }

    /// <summary>
    /// Pings the named entity recognition service to check its availability.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response from the service.</returns>
    public async Task<string> PingAsync()
    {
        try
        {
            var response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (outcome, ts, attempt, ctx) =>
                {
                    if (outcome.Exception != null)
                        Logger?.LogWarning(outcome.Exception, "[RETRY] Exception pinging NLP service on attempt {attempt}", attempt);
                    else if (outcome.Result != null)
                        Logger?.LogWarning("[RETRY] Failed to ping NLP service on attempt {attempt}. Status: {statusCode}", attempt, outcome.Result.StatusCode);
                })
                .ExecuteAsync(() => _client.GetAsync("ping"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to ping NLP service");
            throw;
        }
    }

    /// <summary>
    /// Analyzes a single text corpus for named entities.
    /// </summary>
    /// <param name="corpus">The text corpus to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the named entity collection.</returns>
    public async Task<NamedEntityCollection> AnalyzeSingleAsync(object corpus)
    {
        Logger.LogInformation("Analyzing single corpus");
        var json = JsonSerializer.Serialize(corpus);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (outcome, ts, attempt, ctx) =>
                {
                    if (outcome.Exception != null)
                        Logger?.LogWarning(outcome.Exception, "[RETRY] Exception analyzing single corpus on attempt {attempt}", attempt);
                    else if (outcome.Result != null)
                        Logger?.LogWarning("[RETRY] Failed to analyze single corpus on attempt {attempt}. Status: {statusCode}", attempt, outcome.Result.StatusCode);
                })
                .ExecuteAsync(() => _client.PostAsync("analyzeSingle", content));
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<NamedEntityCollection>(responseBody) ?? new NamedEntityCollection();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to analyze single corpus");
            throw;
        }
    }

    /// <summary>
    /// Analyzes a batch of text corpora for named entities.
    /// </summary>
    /// <param name="corpora">The array of text corpora to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of named entity collections.</returns>
    public async Task<NamedEntityCollection[]> AnalyzeBatchAsync(object[] corpora)
    {
        var json = JsonSerializer.Serialize(corpora);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (outcome, ts, attempt, ctx) =>
                {
                    if (outcome.Exception != null)
                        Logger?.LogWarning(outcome.Exception, "[RETRY] Exception analyzing batch corpus on attempt {attempt}", attempt);
                    else if (outcome.Result != null)
                        Logger?.LogWarning("[RETRY] Failed to analyze batch corpus on attempt {attempt}. Status: {statusCode}", attempt, outcome.Result.StatusCode);
                })
                .ExecuteAsync(() => _client.PostAsync("analyzeBatch", content));
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<NamedEntityCollection[]>(responseBody) ?? Array.Empty<NamedEntityCollection>();
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to analyze batch corpus");
            throw;
        }
    }
}
