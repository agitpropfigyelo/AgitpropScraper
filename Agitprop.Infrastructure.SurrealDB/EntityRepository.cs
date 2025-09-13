using Polly;
using Microsoft.Extensions.Configuration;
using Agitprop.Infrastructure.SurrealDB.Models;
using SurrealDb.Net;
using Microsoft.Extensions.Logging;
using SurrealDb.Net.Models;

namespace Agitprop.Infrastructure.SurrealDB;

public class EntityRepository : IEntityRepository
{
    private readonly ISurrealDbClient _client;
    private readonly ILogger<EntityRepository> _logger;
    private readonly int _retryCount;

    public EntityRepository(ISurrealDbClient client, ILogger<EntityRepository> logger, IConfiguration? configuration = null)
    {
        _client = client;
        _logger = logger;
    _retryCount = configuration?.GetValue<int>("Retry:SurrealDB", 3) ?? 3;
    }

    public Task<IEnumerable<Entity>> GetEntitiesPaginatedAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Entity>> GetEntitiesAsync()
    {
        try
        {
            var res = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (ex, ts, attempt, ctx) =>
                {
                    _logger?.LogWarning(ex, "[RETRY] Exception selecting entities on attempt {attempt}", attempt);
                })
                .ExecuteAsync(() => _client.Select<Entity>("entity"));
            return res;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to select entities from SurrealDB");
            throw;
        }
    }

    public async Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        var recordId = new StringRecordId("entity:" + entityId);
        try
        {
            var res = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (ex, ts, attempt, ctx) =>
                {
                    _logger?.LogWarning(ex, "[RETRY] Exception selecting entity by id on attempt {attempt}", attempt);
                })
                .ExecuteAsync(() => _client.Select<Entity>(recordId));
            return res;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to select entity by id from SurrealDB: {entityId}", entityId);
            throw;
        }
    }

    public async Task<IEnumerable<Article>> GetMentioningArticlesAsync(string entityId, DateTime from, DateTime to)
    {
        var recordId = new StringRecordId("entity:" + entityId);
        var vars = new Dictionary<string, object?>
        {
            { "entityId", recordId },
            { "startDate", from },
            { "endDate", to }
        };
        try
        {
            var response = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (ex, ts, attempt, ctx) =>
                {
                    _logger?.LogWarning(ex, "[RETRY] Exception querying mentioning articles on attempt {attempt}", attempt);
                })
                .ExecuteAsync(() => _client.RawQuery(GetMentioningArticlesQuery, vars as IReadOnlyDictionary<string, object?>));
            return response.FirstOk != null ? response.FirstOk.GetValues<Article>() : Enumerable.Empty<Article>();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to query mentioning articles for entity: {entityId}", entityId);
            throw;
        }
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
        try
        {
            var result = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_retryCount, attempt => TimeSpan.FromSeconds(0.5 * attempt), (ex, ts, attempt, ctx) =>
                {
                    _logger?.LogWarning(ex, "[RETRY] Exception searching entities on attempt {attempt}", attempt);
                })
                .ExecuteAsync(() => _client.RawQuery(SearchEntitiesQuery, new Dictionary<string, object?> { { "input", query } }));
            var entities = result.FirstOk != null ? result.FirstOk.GetValues<EntityFuzzySearchResult>().ToList() : new List<EntityFuzzySearchResult>();
            return entities.Select(e => e.Entity).Where(e => e != null)!;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to search entities in SurrealDB: {query}", query);
            throw;
        }
    }
    private const string SearchEntitiesQuery =
    """
    SELECT $this as Entity, string::similarity::jaro($input, $this.Name) AS Similarity 
    FROM entity ORDER BY Similarity DESC LIMIT 10;
    """;

    private class EntityFuzzySearchResult
    {
    public Entity? Entity { get; set; }
        public double Similarity { get; set; }
    }
}
