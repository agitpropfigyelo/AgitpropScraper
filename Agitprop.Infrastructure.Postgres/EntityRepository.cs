using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Agitprop.Core.Interfaces;
using Agitprop.Core.Models;

namespace Agitprop.Infrastructure.Postgres;

public class EntityRepository : IEntityRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<EntityRepository> _logger;
    private readonly ActivitySource _activitySource = new("Agitprop.Repository.EntityRepository");

    public EntityRepository(
        AppDbContext dbContext,
        ILogger<EntityRepository> logger,
        IConfiguration? configuration = null)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<Entity>> GetEntitiesAsync()
    {
        using var trace = _activitySource.StartActivity("GetEntities", ActivityKind.Internal);
        try
        {
            var results = await _dbContext.Entities.ToListAsync();
            return results.ToCoreModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all entities");
            trace?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Entity>> GetEntitiesPaginatedAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize)
    {
        using var trace = _activitySource.StartActivity("GetEntitiesPaginated", ActivityKind.Internal);
        trace?.SetTag("startDate", startDate.ToString());
        trace?.SetTag("endDate", endDate.ToString());
        trace?.SetTag("page", page);
        trace?.SetTag("pageSize", pageSize);

        try
        {
            var from = DateTime.SpecifyKind(startDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(endDate.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

            var entities = await _dbContext.Entities
                .Where(e => e.Mentions.Any(m =>
                    m.Article.PublishedTime >= from &&
                    m.Article.PublishedTime <= to))
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            trace?.SetTag("resultCount", entities.Count);
            return entities.ToCoreModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve paginated entities mentioned in articles between {startDate} and {endDate}", startDate, endDate);
            trace?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<Entity?> GetEntityByIdAsync(string entityId)
    {
        using var trace = _activitySource.StartActivity("GetEntityById", ActivityKind.Internal);
        trace?.SetTag("entityId", entityId);

        try
        {
            var results = await _dbContext.Entities.FirstOrDefaultAsync(e => e.Id == Guid.Parse(entityId));
            return results?.ToCoreModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve entity by id {entityId}", entityId);
            trace?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Article>> GetMentioningArticlesAsync(string entityId, DateTime from, DateTime to)
    {
        using var trace = _activitySource.StartActivity("GetMentioningArticles", ActivityKind.Internal);
        trace?.SetTag("entityId", entityId);
        trace?.SetTag("from", from.ToString("o"));
        trace?.SetTag("to", to.ToString("o"));

        try
        {
            var result = await _dbContext.Articles
                .Where(a => a.Mentions.Any(m => m.EntityId == Guid.Parse(entityId))
                         && a.PublishedTime >= from
                         && a.PublishedTime <= to)
                .ToListAsync();
            return result.ToCoreModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve mentioning articles for entity {entityId}", entityId);
            trace?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Entity>> SearchEntitiesAsync(string query)
    {
        using var trace = _activitySource.StartActivity("SearchEntities", ActivityKind.Internal);
        trace?.SetTag("query", query);

        try
        {
            var results = await _dbContext.Entities
                .Where(e => EF.Functions.ILike(e.Name, $"%{query}%"))
                .ToListAsync();
            return results.ToCoreModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search entities with query '{query}'", query);
            trace?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}
