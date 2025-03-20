using System.Text;
using System.Text.Json;

using Agitprop.Scraper.Sinks.Newsfeed.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Scraper.Sinks.Newsfeed;

public class NamedEntityRecognizer(HttpClient client, ILogger<NamedEntityRecognizer> logger) : INamedEntityRecognizer
{
    private readonly HttpClient _client = client;
    private ILogger<NamedEntityRecognizer> Logger = logger;

    public async Task<string> PingAsync()
    {
        var response = await _client.GetAsync("ping");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

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
