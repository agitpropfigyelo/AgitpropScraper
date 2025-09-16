using Agitprop.Core.Models;

namespace Agitprop.Core.Interfaces;

public interface ITrendingRepository
{
    Task<IEnumerable<Entity>> GetTrendingEntitiesAsync(DateTime fromDate, DateTime toDate);
}
