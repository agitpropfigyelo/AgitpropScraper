using System;

using Agitprop.Infrastructure.SurrealDB.Models;

using Microsoft.Extensions.Logging;

using SurrealDb.Net;

namespace Agitprop.Infrastructure.SurrealDB;

public interface ITrendingRepository
{
    Task<IEnumerable<Entity>> GetTrendingEntitiesAsync(DateTime fromDate, DateTime toDate);
}
