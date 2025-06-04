using Agitprop.Infrastructure.SurrealDB.Models;
using SurrealDb.Net;
using SurrealDb.Net.Models.Response;
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

    public async Task<IEnumerable<Entity>> GetEntitiesAsync(string? query = null)
    {
        string surrealQuery = string.IsNullOrWhiteSpace(query)
            ? "SELECT * FROM entity;"
            : "SELECT * FROM entity WHERE string::lower(Name) CONTAINS string::lower($q);";
        var parameters = string.IsNullOrWhiteSpace(query) ? null : new Dictionary<string, object?> { { "q", query } };
        var result = await _client.RawQuery(surrealQuery, parameters);
        return result.FirstOk.GetValues<Entity>();
    }

    public async Task<IEnumerable<(Article Article, Mentions Mention)>> GetMentionsAsync(string entityId, DateTime from, DateTime to)
    {
        string surrealQuery = @"
            SELECT article.*, mention.* FROM mentions AS mention
            <-[in]- article
            WHERE mention.out = $entityId
            AND mention.Date >= $from AND mention.Date <= $to
        ";
        var parameters = new Dictionary<string, object?>
        {
            { "entityId", entityId },
            { "from", from },
            { "to", to }
        };
        var result = await _client.RawQuery(surrealQuery, parameters);
        var list = new List<(Article, Mentions)>();
        foreach (var row in result.FirstOk.GetValues<dynamic>())
        {
            var article = row["article"].ToObject<Article>();
            var mention = row["mention"].ToObject<Mentions>();
            list.Add((article, mention));
        }
        return list;
    }

    public async Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        var result = await _client.Select<Entity>(new StringRecordId(entityId));
        return result;
    }

    public Task<IEnumerable<(DateTime date, int count)>> GetTrendingMentionsAsync(string entityId, DateTime from, DateTime to)
    {
        throw new NotImplementedException();
    }
}
