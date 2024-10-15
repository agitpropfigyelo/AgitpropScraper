using System.Text;
using System.Text.Json;
using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public class NamedEntityRecognizer : INamedEntityRecognizer
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private ILogger<NamedEntityRecognizer> Logger;


    public NamedEntityRecognizer(IConfiguration configuration, ILogger<NamedEntityRecognizer> logger)
    {
        _client = new HttpClient();
        _baseUrl = configuration["NERbaseUrl"];
        Logger = logger;
    }

    public async Task<string> PingAsync()
    {
        var response = await _client.GetAsync(_baseUrl + "/ping");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<NamedEntityCollection> AnalyzeSingleAsync(object corpus)
    {
        var json = JsonSerializer.Serialize(corpus);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_baseUrl + "/analyzeSingle", content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<NamedEntityCollection>(responseBody);
    }

    public async Task<NamedEntityCollection[]> AnalyzeBatchAsync(object[] corpora)
    {
        var json = JsonSerializer.Serialize(corpora);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(_baseUrl + "/analyzeBatch", content);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<NamedEntityCollection[]>(responseBody);
    }
}