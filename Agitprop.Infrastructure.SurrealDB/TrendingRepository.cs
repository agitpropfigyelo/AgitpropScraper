
using Agitprop.Infrastructure.SurrealDB.Models;

namespace Agitprop.Infrastructure.SurrealDB;

public class TrendingRepository : ITrendingRepository
{
    public Task<IEnumerable<Entity>> GetTrendingEntitiesAsync(DateTime fromDate, DateTime toDate)
    {
        throw new NotImplementedException();
    }
}
