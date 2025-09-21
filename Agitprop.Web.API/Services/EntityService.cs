using System.Text.RegularExpressions;
using Agitprop.Core.Interfaces;
using Agitprop.Web.Api.DTOs;

namespace Agitprop.Web.Api.Services;

public partial class EntityService : IEntityService
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

    public async Task<List<EntityDto>> GetEntitiesAsync(DateOnly startDate, DateOnly endDate, int page, int pageSize)
    {
        try
        {
            var result = await _entityRepository.GetEntitiesPaginatedAsync(startDate, endDate, page, pageSize);
            return result.ToEntityDtos().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching entities for time range {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<EntityDetailsDto?> GetEntityDetailsAsync(Guid id, DateOnly startDate, DateOnly endDate)
    {
        try
        {
            var result = await _entityRepository.GetEntityByIdAsync(id.ToString());
            return result?.ToEntityDetailsDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching entity details for id {EntityId}", id);
            throw;
        }
    }

    public async Task<List<ArticleDto>> GetMentioningArticlesAsync(Guid id, DateOnly startDate, DateOnly endDate, int page, int pageSize)
    {
        try
        {
            var articles = await _entityRepository.GetMentioningArticlesAsync(id.ToString(), startDate, endDate, page, pageSize);
            return articles.Select(a => new ArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Url = a.Url,
                Source = a.Source,
                PublishedAt = DateOnly.FromDateTime(a.PublishedAt),
                Sentiment = a.Sentiment,
                MentionedEntities = a.MentionedEntities
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching articles mentioning entity {EntityId}", id);
            throw;
        }
    }

    public async Task<List<NetworkItemDto>> GetNetworkDetailsAsync(Guid id, DateOnly? startDate, DateOnly? endDate, int limit)
    {
        try
        {
            var networkItems = await _entityRepository.GetEntityNetworkAsync(id.ToString(), startDate, endDate, limit);
            return networkItems.Select(n => new NetworkItemDto
            {
                EntityId = Guid.Parse(n.Id),
                Name = n.Name,
                EntityType = n.EntityType,
                CooccurrenceCount = n.CooccurrenceCount,
                AverageSentiment = n.AverageSentiment
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching network details for entity {EntityId}", id);
            throw;
        }
    }

    public async Task<List<EntityDto>> SearchForEntity(string query)
    {
        try
        {
            if (!queryValidatingRegEx().IsMatch(query))
            {
                throw new ArgumentException("Invalid query format", nameof(query));
            }

            var searchResults = await _entityRepository.SearchEntitiesAsync(query);
            return searchResults.ToEntityDtos().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for entities with query: {Query}", query);
            throw;
        }
    }
}
