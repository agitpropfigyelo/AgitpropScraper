namespace Agitprop.Core.Interfaces;

public interface IScraperConfigStore
{
    Task CreateConfigAsync(ScraperConfig config);

    Task<ScraperConfig> GetConfigAsync();
}
