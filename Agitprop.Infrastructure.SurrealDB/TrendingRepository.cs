
using Agitprop.Core.Interfaces;
using Agitprop.Core.Models;

using Microsoft.Extensions.Logging;

using SurrealDb.Net;

namespace Agitprop.Infrastructure.SurrealDB;

public class TrendingRepository : ITrendingRepository
{
    private readonly ISurrealDbClient _client;
    private readonly ILogger<EntityRepository> _logger;

    public TrendingRepository(ISurrealDbClient client, ILogger<EntityRepository> logger)
    {
        _client = client;
        _logger = logger;
    }
    public Task<IEnumerable<Entity>> GetTrendingEntitiesAsync(DateTime fromDate, DateTime toDate)
    {
        throw new NotImplementedException();
    }
}
