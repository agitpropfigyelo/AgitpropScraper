namespace Agitprop.Infrastructure.ProxyService
{
    public class ProxyRevalidatorBackgroundService : BackgroundService
    {
        private readonly IProxyManager _manager;
        private readonly TimeSpan _interval;
        private readonly ILogger<ProxyRevalidatorBackgroundService> _logger;

        public ProxyRevalidatorBackgroundService(IProxyManager manager, TimeSpan interval, ILogger<ProxyRevalidatorBackgroundService> logger)
        {
            _manager = manager;
            _interval = interval;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProxyRevalidatorBackgroundService started with interval {IntervalMinutes} minutes", _interval.TotalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Starting scheduled proxy refresh");
                    await _manager.RefreshAllAsync(stoppingToken);
                    _logger.LogDebug("Completed scheduled proxy refresh");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Proxy refresh cancelled due to stop request");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during scheduled proxy refresh");
                }

                try
                {
                    _logger.LogTrace("Waiting {IntervalMinutes} minutes until next refresh", _interval.TotalMinutes);
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException) { break; }
            }

            _logger.LogInformation("ProxyRevalidatorBackgroundService stopped");
        }
    }
}