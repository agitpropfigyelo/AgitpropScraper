using Agitprop.Web.Api.DTOs;

namespace Agitprop.Web.Api.Services;

public interface IEntityService
{
    Task<List<EntityDto>> GetEntitiesAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize);
    Task<EntityDetailsDto?> GetEntityDetailsAsync(Guid id, DateOnly startDate, DateOnly endDate);
    Task<List<ArticleDto>> GetMentioningArticlesAsync(Guid id, DateOnly startDate, DateOnly endDate, int page, int pageSize);
    Task<List<NetworkItemDto>> GetNetworkDetailsAsync(Guid id, DateOnly? startDate, DateOnly? endDate, int limit);
    Task<List<EntityDto>> SearchForEntity(string query);
}