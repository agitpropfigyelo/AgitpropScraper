using System.Text.RegularExpressions;

using Agitprop.Infrastructure.SurrealDB;

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

    [GeneratedRegex(@"^[\p{L}\d _-]{3,100}$")]
    private static partial Regex queryValidatingRegEx();

    internal async Task<List<EntityDto>> GetEntitiesAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize)
    {
        var result= await _entityRepository.GetEntitiesPaginatedAsync(startDate, endDate, page, pageSize);
        return result.ToEntityDtos().ToList();
    }

    internal async Task<EntityDto> GetEntityDetailsAsync(Guid id, DateOnly startDate, DateOnly endDate)
    {
        var result = await _entityRepository.GetEntityByIdAsync(id.ToString());
        return result.ToEntityDto();
    }

    internal async Task<List<ArticleDto>> GetMentioningArticlesAsync(Guid id, DateOnly startDate, DateOnly endDate, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    internal async Task<List<NetworkItemDto>> GetNetworkDetailskAsync(Guid id, DateOnly? startDate, DateOnly? endDate, int limit)
    {
        throw new NotImplementedException();
    }

    internal async Task<List<EntityDto>> SearchForEntity(string query)
    {
        throw new NotImplementedException();
    }
}
