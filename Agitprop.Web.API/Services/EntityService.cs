using Agitprop.Web.Api.Models;
using Agitprop.Web.Api.Repositories;

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
        => await _entityRepository.GetByIdAsync(id);

    public async Task<IEnumerable<Mention>> GetMentionsAsync(string entityId, DateTime from, DateTime to)
        => await _entityRepository.GetMentionsAsync(entityId, from, to);

    public async Task<IEnumerable<(DateTime date, int count)>> GetTrendingMentionsAsync(string entityId, DateTime from, DateTime to)
        => await _entityRepository.GetTrendingMentionsAsync(entityId, from, to);
}
