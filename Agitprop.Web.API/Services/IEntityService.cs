using Agitprop.Web.Api.DTOs.Responses;

namespace Agitprop.Web.Api.Services;

public interface IEntityService
{
    Task<List<EntityResponse>> GetEntitiesAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize);
    Task<EntityDetailsResponse?> GetEntityDetailsAsync(Guid id, DateOnly startDate, DateOnly endDate);
    Task<List<ArticleResponse>> GetMentioningArticlesAsync(Guid id, DateOnly startDate, DateOnly endDate, int page, int pageSize);
    Task<List<NetworkItemResponse>> GetNetworkDetailsAsync(Guid id, DateOnly? startDate, DateOnly? endDate, int limit);
    Task<List<EntityResponse>> SearchForEntity(string query);
}