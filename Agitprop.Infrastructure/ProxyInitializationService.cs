using System;
using System.Threading;
using System.Threading.Tasks;

using Agitprop.Core.Interfaces;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

/// <summary>
/// Hosted service that initializes the proxy pool on application startup.
/// </summary>
public class ProxyInitializationService : IHostedService
{
    private readonly IProxyPool _proxyPool;
    private readonly ILogger<ProxyInitializationService> _logger;

    public ProxyInitializationService(IProxyPool proxyPool, ILogger<ProxyInitializationService> logger)
    {
        _proxyPool = proxyPool;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting proxy pool initialization service");
        
        try
        {
            _logger.LogInformation("Initializing proxy pool - this will block until {Target} proxies are ready or timeout occurs", 
                "25 (configurable)");
            
            await _proxyPool.InitializeAsync(cancellationToken);
            
            _logger.LogInformation("Proxy pool initialization service completed successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Proxy pool initialization was cancelled - this may be expected if startup timeout was reached");
            // Don't rethrow - let the application continue with whatever proxies were found
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize proxy pool during startup");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
