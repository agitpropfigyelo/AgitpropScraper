using Agitprop.Web.Api.Repositories;

namespace Agitprop.Web.Api.Services;

public class TrendingService
{
    private readonly ITrendingRepository _trendingRepository;
    private readonly ILogger<TrendingService> _logger;

    public TrendingService(ITrendingRepository trendingRepository, ILogger<TrendingService> logger)
    {
        _trendingRepository = trendingRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<(string entityId, string entityName, DateTime date, int count)>> GetTrendingEntitiesAsync(DateTime from, DateTime to)
        => await _trendingRepository.GetTrendingEntitiesAsync(from, to);
}
