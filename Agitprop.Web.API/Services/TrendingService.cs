using Agitprop.Infrastructure.SurrealDB;

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
    {
        throw new NotImplementedException("This method is not implemented yet.");
        // var entities = await _trendingRepository.GetTrendingEntitiesAsync(from, to);
        // return entities.Select(e => (e.Id, e.Name, e.Date, e.Count));
    }
}
