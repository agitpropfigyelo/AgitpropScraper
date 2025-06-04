using Agitprop.Infrastructure.SurrealDB;
using Agitprop.Infrastructure.SurrealDB.Models;

namespace Agitprop.Web.Api.Services;

public class EntityService
{
    private readonly IEntityRepository _entityRepository;
    private readonly ILogger<EntityService> _logger;

    public EntityService(IEntityRepository entityRepository, ILogger<EntityService> logger)
    {
        _entityRepository = entityRepository;
        _logger = logger;
    }

    public async Task<Entity?> GetEntityAsync(string id)
        => await _entityRepository.GetEntityByIdAsync(id);

    public async Task<IEnumerable<Mentions>> GetMentionsAsync(string entityId, DateTime from, DateTime to)
        => (await _entityRepository.GetMentionsAsync(entityId, from, to)).Select(x => x.Mention);

    public async Task<IEnumerable<(DateTime date, int count)>> GetTrendingMentionsAsync(string entityId, DateTime from, DateTime to)
        => await _entityRepository.GetTrendingMentionsAsync(entityId, from, to);
}
