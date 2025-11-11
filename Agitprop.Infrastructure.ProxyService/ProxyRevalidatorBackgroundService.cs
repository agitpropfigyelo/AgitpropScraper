namespace Agitprop.Infrastructure.ProxyService
{
    public class ProxyRevalidatorBackgroundService : BackgroundService
    {
        private readonly IProxyManager _manager;
        private readonly TimeSpan _interval;

        public ProxyRevalidatorBackgroundService(IProxyManager manager, TimeSpan interval)
        {
            _manager = manager;
            _interval = interval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _manager.RefreshAllAsync(stoppingToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    // log
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException) { break; }
            }
        }
    }
}