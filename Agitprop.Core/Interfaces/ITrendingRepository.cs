using Agitprop.Core.Models;

namespace Agitprop.Core.Interfaces;

public interface ITrendingRepository
{
    IEnumerable<Entity> GetTrendingEntitiesAsync(DateOnly fromDate, DateOnly toDate, int topN = 10);
}
