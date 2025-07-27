using Agitprop.Infrastructure.SurrealDB.Models;
using SurrealDb.Net;
using Microsoft.Extensions.Logging;
using SurrealDb.Net.Models;

namespace Agitprop.Infrastructure.SurrealDB;

public class EntityRepository : IEntityRepository
{
    private readonly ISurrealDbClient _client;
    private readonly ILogger<EntityRepository> _logger;

    public EntityRepository(ISurrealDbClient client, ILogger<EntityRepository> logger)
    {
        _client = client;
        _logger = logger;
    }

    public Task<IEnumerable<Entity>> GetEntitiesPaginatedAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Entity>> GetEntitiesAsync()
    {
        var res = await _client.Select<Entity>("entity");
        return res;
    }

    public async Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        var recordId=new StringRecordId("entity:"+entityId);
        var res = await _client.Select<Entity>(recordId);
        return res;
    }


    public async Task<IEnumerable<Article>> GetMentioningArticlesAsync(string entityId, DateTime from, DateTime to)
    {
        throw new NotImplementedException("This method is not implemented yet.");
    }

    public async Task<IEnumerable<Entity>> SearchEntitiesAsync(string query)
    {
        var result = await _client.RawQuery("SELECT $this, string::similarity::jaro($input, $this.Name) AS similarity FROM entity ORDER BY similarity DESC LIMIT 10;",
            new Dictionary<string, object?> { { "input", query } });
        return result.FirstOk.GetValues<Entity>();
    }
}
