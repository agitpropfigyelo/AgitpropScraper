using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public class RotatingHttpClientPool
{
    private readonly IProxyPool _pool;
    private readonly ILogger<RotatingHttpClientPool>? _logger;
    private readonly ActivitySource _activitySource = new("Agitprop.RotatingHttpClientPool");
    private readonly List<string> _defaultUserAgents;
    private readonly int _maxRetries;
    private readonly double _successConfidenceThreshold;

    public RotatingHttpClientPool(IProxyPool pool, ILogger<RotatingHttpClientPool>? logger = null, int maxRetries = 10, double successConfidenceThreshold = 0.9)
    {
        _pool = pool;
        _logger = logger;
        _maxRetries = maxRetries;
        _successConfidenceThreshold = successConfidenceThreshold;
        _defaultUserAgents =
        [
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/118.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36 Edg/117.0.2045.47",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Safari/605.1.15",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:109.0) Gecko/20100101 Firefox/118.0",
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36",
            "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/118.0",
            "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Mobile Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.6 Mobile/15E148 Safari/604.1"
        ];
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        using var activity = _activitySource.StartActivity("SendAsync", ActivityKind.Internal);
        
        // Clone the request to avoid issues
        var requestClone = CloneRequest(request);
        
        try
        {
            activity?.SetTag("http.method", request.Method.Method);
            activity?.SetTag("http.url", request.RequestUri?.ToString() ?? string.Empty);

            // Randomize headers: user-agent, accept-language
            if (!requestClone.Headers.UserAgent.Any())
            {
                var ua = _defaultUserAgents[new Random().Next(_defaultUserAgents.Count)];
                requestClone.Headers.UserAgent.ParseAdd(ua);
                _logger?.LogDebug("Assigned User-Agent '{UA}' to request {Method} {Url}", ua, requestClone.Method, requestClone.RequestUri);
                activity?.SetTag("http.user_agent", ua);
            }

            if (!requestClone.Headers.AcceptLanguage.Any())
            {
                requestClone.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            }

            // Get proxy invoker (this will block and retry internally if needed)
            var result = await _pool.GetRandomInvokerAsync(ct);
            if (result == null)
            {
                var exception = new InvalidOperationException("No proxy invoker available");
                _logger?.LogError(exception, "No proxy invoker available when sending {Method} {Url}", requestClone.Method, requestClone.RequestUri);
                activity?.SetStatus(ActivityStatusCode.Error, "No proxy invoker available");
                throw exception;
            }

            var (invoker, address) = result.Value;
            activity?.SetTag("proxy.address", address);
            
            _logger?.LogDebug("Selected proxy {Proxy} for request {Method} {Url}", 
                address, requestClone.Method, requestClone.RequestUri);

            if (invoker is null)
            {
                var exception = new InvalidOperationException("Selected invoker was null");
                _logger?.LogError(exception, "Selected invoker was null for proxy {Proxy} when sending {Method} {Url}", 
                    address, requestClone.Method, requestClone.RequestUri);
                activity?.SetStatus(ActivityStatusCode.Error, "Selected invoker was null");
                throw exception;
            }

            // Send via invoker
            try
            {
                var resp = await invoker.SendAsync(requestClone, ct);
                activity?.SetTag("http.status_code", (int)resp.StatusCode);
                activity?.SetStatus(ActivityStatusCode.Ok);
                _logger?.LogDebug("Request {Method} {Url} via proxy {Proxy} returned {StatusCode}", 
                    requestClone.Method, requestClone.RequestUri, address, resp.StatusCode);
                return resp;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Request {Method} {Url} via proxy {Proxy} failed", 
                    requestClone.Method, requestClone.RequestUri, address);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                // Mark proxy as dead
                if (_pool is ProxyPoolService)
                {
                    _logger?.LogWarning("Marking proxy {Proxy} as dead due to exception: {Message}", address, ex.Message);
                    await _pool.MarkDeadAsync(address);
                }

                throw;
            }
        }
        catch (Exception ex)
        {
            // Log and rethrow
            _logger?.LogError(ex, "Failed to send request {Method} {Url}", requestClone.Method, requestClone.RequestUri);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
    
    private HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        
        // Copy headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        // Copy content if present
        if (request.Content != null)
        {
            var content = request.Content.ReadAsStreamAsync().Result;
            clone.Content = new StreamContent(content);
            
            // Copy content headers
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        
        return clone;
    }
}
