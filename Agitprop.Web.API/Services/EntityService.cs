using System.Text.RegularExpressions;

using Agitprop.Infrastructure.SurrealDB;
using Agitprop.Infrastructure.SurrealDB.Models;

namespace Agitprop.Web.Api.Services;

public partial class EntityService
{
    private readonly IEntityRepository _entityRepository;
    private readonly ILogger<EntityService> _logger;

    public EntityService(IEntityRepository entityRepository, ILogger<EntityService> logger)
    {
        _entityRepository = entityRepository;
        _logger = logger;
    }

    public async Task<Entity?> GetEntityAsync(string id)
    {
        _logger.LogInformation("Getting entity with id: {EntityId}", id);
        var entity = await _entityRepository.GetEntityByIdAsync(id);
        if (entity == null)
            _logger.LogWarning("Entity not found: {EntityId}", id);
        return entity;
    }

    public async Task<IEnumerable<ArticleDto>> GetMentioningArticlesAsync(string entityId, DateTime? from, DateTime? to)
    {
        _logger.LogInformation("Getting mentioning articles for entity: {EntityId}, from: {From}, to: {To}", entityId, from, to);
        var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
        var toDate = to ?? DateTime.UtcNow;
        var result = (await _entityRepository.GetMentioningArticlesAsync(entityId, fromDate, toDate)).Select(a => new ArticleDto
        {
            ArticleUrl = a.Url,
            ArticlePublishedTime = a.PublishedTime
        });
        return result;
    }

    internal async Task<IEnumerable<EntityDto>> GetEntitiesAsync(string? query)
    {
        _logger.LogInformation("Getting entities with query: {Query}", query);
        if (query is null)
        {
            var allEntities = await _entityRepository.GetEntitiesAsync();
            return allEntities.Select(e => new EntityDto { Id = e.Id.DeserializeId<string>(), Name = e.Name });
        }

        var regex = queryValidatingRegEx();
        if (!regex.IsMatch(query))
        {
            _logger.LogWarning("Invalid query string: {Query}", query);
            throw new ArgumentException("The query is invalid as it contains invalid characters.");
        }
        var entities = await _entityRepository.SearchEntitiesAsync(query);
        return entities.Select(e => new EntityDto { Id = e.Id.ToString(), Name = e.Name });

    }

    [GeneratedRegex(@"^[\p{L}\d _-]{3,100}$")]
    private static partial Regex queryValidatingRegEx();
}
