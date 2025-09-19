using System;

using Agitprop.Core.Interfaces;
using Agitprop.Core.Models;

namespace Agitprop.Infrastructure.Postgres;

public class TrendingRepository : ITrendingRepository
{
    public Task<IEnumerable<Entity>> GetTrendingEntitiesAsync(DateTime fromDate, DateTime toDate)
    {
        throw new NotImplementedException();
    }
}
