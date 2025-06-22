using Agitprop.Infrastructure.SurrealDB.Models;
using SurrealDb.Net;
using Microsoft.Extensions.Logging;
using SurrealDb.Net.Models;
using System.Collections;

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

    public async Task<IEnumerable<Entity>> GetEntitiesAsync(string? query = null)
    {
        string surrealQuery = string.IsNullOrWhiteSpace(query)
            ? "SELECT * FROM entity;"
            : "SELECT * FROM entity WHERE string::lower(Name) CONTAINS string::lower($q);";
        var parameters = string.IsNullOrWhiteSpace(query) ? null : new Dictionary<string, object?> { { "q", query } };
        var result = await _client.RawQuery(surrealQuery, parameters);
        return result.FirstOk.GetValues<Entity>();
    }

    public async Task<IEnumerable<Entity>> GetEntitiesAsync()
    {
        var res = await _client.Select<Entity>("entity");
        return res;
    }

    public Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Article>> GetMentioningArticlesAsync(string entityId, DateTime from, DateTime to)
    {
        var result = await _client.Select();
    }

    public async Task<IEnumerable<Entity>> SearchEntitiesAsync(string query)
    {
        var result = await _client.RawQuery("SELECT $this, string::similarity::jaro($input, $this.Name) AS similarity FROM entity ORDER BY similarity DESC LIMIT 10;",
            new Dictionary<string, object?> { { "input", query } });
        return result.FirstOk.GetValues<Entity>();
    }
}
