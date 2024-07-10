using Agitprop.Core;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure;


public class InMemoryScraperConfigStore : IScraperConfigStore
{
    private ScraperConfig _config;

    public Task CreateConfigAsync(ScraperConfig config)
    {
        _config = config;
        return Task.CompletedTask;
    }

    public Task<ScraperConfig> GetConfigAsync()
    {
        return Task.FromResult(_config);
    }
}
