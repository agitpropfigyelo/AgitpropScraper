using Agitprop.Core.Models;

namespace Agitprop.Core.Interfaces
{
    public interface IEntityRepository
    {
        Task<IEnumerable<Entity>> GetEntitiesAsync();
        Task<IEnumerable<Article>> GetMentioningArticlesAsync(string entityId, DateOnly from, DateOnly to);
        Task<Entity?> GetEntityByIdAsync(string entityId);
        Task<IEnumerable<Entity>> SearchEntitiesAsync(string query);
        Task<IEnumerable<Entity>> GetEntitiesPaginatedAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize);
    }
}
