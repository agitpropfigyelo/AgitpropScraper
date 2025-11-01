using System;
using System.Net.Sockets;
using System.Diagnostics;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public class RotatingHttpClientPool
{
    private readonly IProxyPool _pool;
    private readonly ILogger<RotatingHttpClientPool>? _logger;
    private readonly ActivitySource _activitySource = new("Agitprop.RotatingHttpClientPool");
    private readonly List<string> _defaultUserAgents;

    public RotatingHttpClientPool(IProxyPool pool, ILogger<RotatingHttpClientPool>? logger = null)
    {
        _pool = pool;
        _logger = logger;
        _defaultUserAgents = new List<string>
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36",
            // adj hozzá további UA-ket...
        };
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        using var activity = _activitySource.StartActivity("SendAsync", ActivityKind.Internal);
        try
        {
            activity?.SetTag("http.method", request.Method.Method);
            activity?.SetTag("http.url", request.RequestUri?.ToString() ?? string.Empty);

            // header randomizáció: user-agent, accept-language, accept
            if (!request.Headers.UserAgent.Any())
            {
                var ua = _defaultUserAgents[new Random().Next(_defaultUserAgents.Count)];
                request.Headers.UserAgent.ParseAdd(ua);
                _logger?.LogDebug("Assigned User-Agent '{UA}' to request {Method} {Url}", ua, request.Method, request.RequestUri);
                activity?.SetTag("http.user_agent", ua);
            }

            if (!request.Headers.AcceptLanguage.Any())
            {
                request.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            }

            // pick invoker
            var result = await _pool.GetRandomInvokerAsync(ct);
            if (result == null)
            {
                var exception = new InvalidOperationException("No proxy invoker available");
                _logger?.LogError(exception,"No proxy invoker available when sending {Method} {Url}", request.Method, request.RequestUri);
                activity?.SetStatus(ActivityStatusCode.Error, "No proxy invoker available");
                throw new InvalidOperationException("No proxy invoker available");
            }

            var (invoker, address) = result.Value;
            activity?.SetTag("proxy.address", address);
            _logger?.LogDebug("Selected proxy {Proxy} for request {Method} {Url}", address, request.Method, request.RequestUri);

            if (invoker is null)
            {
                _logger?.LogError("Selected invoker was null for proxy {Proxy} when sending {Method} {Url}", address, request.Method, request.RequestUri);
                activity?.SetStatus(ActivityStatusCode.Error, "Selected invoker was null");
                throw new InvalidOperationException("Selected invoker was null");
            }

            // send via invoker
            try
            {
                var resp = await invoker.SendAsync(request, ct);
                activity?.SetTag("http.status_code", (int)resp.StatusCode);
                activity?.SetStatus(ActivityStatusCode.Ok);
                _logger?.LogDebug("Request {Method} {Url} via proxy {Proxy} returned {StatusCode}", request.Method, request.RequestUri, address, resp.StatusCode);
                return resp;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Request {Method} {Url} via proxy {Proxy} failed", request.Method, request.RequestUri, address);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

                // mark dead using proxy address from the pool
                if (invoker is not null && _pool is ProxyPoolService)
                {
                    _logger?.LogWarning("Marking proxy {Proxy} as dead due to exception: {Message}", address, ex.Message);
                    await _pool.MarkDeadAsync(address);
                }

                throw;
            }
        }
        finally
        {
            // nothing else required here for now
        }
    }
}
