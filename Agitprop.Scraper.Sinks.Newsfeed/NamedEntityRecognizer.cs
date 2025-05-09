using System.Text;
using System.Text.Json;

using Agitprop.Scraper.Sinks.Newsfeed.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Scraper.Sinks.Newsfeed;

/// <summary>
/// Provides functionality for recognizing named entities in text using an external service.
/// </summary>
public class NamedEntityRecognizer(HttpClient client, ILogger<NamedEntityRecognizer> logger) : INamedEntityRecognizer
{
    private readonly HttpClient _client = client;
    private ILogger<NamedEntityRecognizer> Logger = logger;

    /// <summary>
    /// Pings the named entity recognition service to check its availability.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response from the service.</returns>
    public async Task<string> PingAsync()
    {
        var response = await _client.GetAsync("ping");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
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
        var response = await _client.PostAsync("analyzeSingle", content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<NamedEntityCollection>(responseBody);
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
        var response = await _client.PostAsync("analyzeBatch", content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<NamedEntityCollection[]>(responseBody);
    }
}
