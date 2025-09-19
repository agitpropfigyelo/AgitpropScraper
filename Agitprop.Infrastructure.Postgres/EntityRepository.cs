using System;

using Agitprop.Core.Interfaces;
using Agitprop.Core.Models;

namespace Agitprop.Infrastructure.Postgres;

public class EntityRepository : IEntityRepository
{
    public Task<IEnumerable<Entity>> GetEntitiesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Entity>> GetEntitiesPaginatedAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Article>> GetMentioningArticlesAsync(string entityId, DateTime from, DateTime to)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Entity>> SearchEntitiesAsync(string query)
    {
        throw new NotImplementedException();
    }
}
