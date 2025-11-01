using System.Diagnostics;
using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure.ProxyProviders;

public class AdvancedNameProxyProvider : IProxyProvider
{
    private readonly ILogger<AdvancedNameProxyProvider>? _logger;
    private readonly ActivitySource _activitySource = new("Agitprop.Infrastructure.ProxyProviders.AdvancedNameProxyProvider");
    private const string _sourceUri = "https://advanced.name/freeproxy/690657397c699?type=https";
    private readonly HttpClient _http;

    public AdvancedNameProxyProvider(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<string>> FetchProxyAddressesAsync()
    {
         using var activity = _activitySource.StartActivity("FetchProxyAddressesAsync", ActivityKind.Internal);
        activity?.SetTag("proxy.source_uri", _sourceUri.ToString());
        try
        {
            var res = await _http.GetAsync(_sourceUri, HttpCompletionOption.ResponseHeadersRead);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            var addresses = json.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // var response = JsonSerializer.Deserialize<ProxyDto>(json);
            // var addresses = response?.Proxies.Select(p => $"{p.Ip}:{p.Port}") ?? Enumerable.Empty<string>();
            activity?.SetTag("proxy.fetched_count", addresses.Count());
            _logger?.LogInformation("Fetched {Count} proxy addresses from {Url}", addresses.Count(), _sourceUri);
            return addresses;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to fetch proxy addresses from {Url}", _sourceUri);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
