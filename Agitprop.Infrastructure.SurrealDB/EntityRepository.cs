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
        var recordId = new StringRecordId("entity:" + entityId);
        var res = await _client.Select<Entity>(recordId);
        return res;
    }

    public async Task<IEnumerable<Article>> GetMentioningArticlesAsync(string entityId, DateTime from, DateTime to)
    {
        var recordId = new StringRecordId("entity:" + entityId);

        var vars = new Dictionary<string, object>
        {
            { "entityId", recordId },
            { "startDate", from },
            { "endDate", to }
        };

        SurrealDb.Net.Models.Response.SurrealDbResponse response = await _client.RawQuery(GetMentioningArticlesQuery, vars);
        return response.FirstOk.GetValues<Article>();
    }
    private const string GetMentioningArticlesQuery =
    """
    SELECT id, in.PublishedTime
    FROM mentions
    WHERE out = $entityId
    AND $startDate < in.PublishedTime
    AND $endDate > in.PublishedTime
    """;

    public async Task<IEnumerable<Entity>> SearchEntitiesAsync(string query)
    {
        var result = await _client.RawQuery(SearchEntitiesQuery, new Dictionary<string, object?> { { "input", query } });
        var entities = result.FirstOk.GetValues<EntityFuzzySearchResult>().ToList();
        return entities.Select(e => e.Entity);
    }
    private const string SearchEntitiesQuery =
    """
    SELECT $this as Entity, string::similarity::jaro($input, $this.Name) AS Similarity 
    FROM entity ORDER BY Similarity DESC LIMIT 10;
    """;

    private class EntityFuzzySearchResult
    {
        public Entity Entity { get; set; }
        public double Similarity { get; set; }
    }
}
