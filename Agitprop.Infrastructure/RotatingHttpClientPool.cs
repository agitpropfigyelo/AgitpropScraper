using System;
using System.Net.Sockets;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Configuration;

namespace Agitprop.Infrastructure;

public class RotatingHttpClientPool
{
    private readonly IProxyPool _pool;
    private readonly IConfiguration _config;
    private readonly List<string> _defaultUserAgents;

    public RotatingHttpClientPool(IProxyPool pool, IConfiguration config)
    {
        _pool = pool;
        _config = config;
        _defaultUserAgents = new List<string>
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36",
            // adj hozzá további UA-ket...
        };
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        // header randomizáció: user-agent, accept-language, accept
        if (!request.Headers.UserAgent.Any())
            request.Headers.UserAgent.ParseAdd(_defaultUserAgents[new Random().Next(_defaultUserAgents.Count)]);

        if (!request.Headers.AcceptLanguage.Any())
            request.Headers.AcceptLanguage.ParseAdd("en-US,en;q=0.9");

        // pick invoker
        var result = await _pool.GetRandomInvokerAsync(ct);
        if (result == null)
            throw new InvalidOperationException("No proxy invoker available");

        var (invoker, address) = result.Value;

        // send via invoker
        try
        {
            return await invoker.SendAsync(request, ct);
        }
        catch (Exception ex)
        {
            // mark dead using proxy address from the pool
            if (invoker is not null && _pool is ProxyPoolService poolService)
            {
                await _pool.MarkDeadAsync(address);
            }

            throw;
        }


    }
}
