using Microsoft.AspNetCore.Mvc;
using Agitprop.Core.Interfaces;
using Agitprop.Web.Api.DTOs.Requests;
using Agitprop.Web.Api;
using Agitprop.Web.Api.DTOs.Responses;
using Agitprop.Web.Api.DTOs;
using Agitprop.Web.Api.Models;

namespace Agitprop.Api.Controllers;

/// <summary>
/// Provides endpoints for browsing and analyzing entities.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EntitiesController : ControllerBase
{
    private readonly ILogger<EntitiesController> _logger;
    private readonly IEntityRepository _entityRepository;

    public EntitiesController(
        ILogger<EntitiesController> logger,
        IEntityRepository repository)
    {
        _logger = logger;
        _entityRepository = repository;
    }

    /// <summary>
    /// Returns a paginated list of entities mentioned in articles within the given date range.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedEntitiesResponse>> GetEntitiesPaginatedAsync(
        [FromQuery] EntitiesPaginatedRequest request,
        CancellationToken cancellationToken = default)
    {
        var entities = await _entityRepository.GetEntitiesPaginatedAsync(
            request.StartDate,
            request.EndDate,
            request.Page,
            request.PageSize);

        var response = new PaginatedEntitiesResponse
        {
            Entities = entities.ToEntityDtos(),
            Page = request.Page
        };

        return Ok(response);
    }

    /// <summary>
    /// Returns details for a specific entity.
    /// </summary>
    [HttpGet("{entityId}/details")]
    public async Task<ActionResult<EntityDetailsResponse>> GetEntityDetailsAsync(
        string entityId,
        [FromQuery] EntityDetailsRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _entityRepository.GetEntityByIdAsync(entityId);

        if (entity == null)
            return NotFound();

        var dto = new EntityDetailsResponse
        {
            EntityId = entity.Id,
            Name = entity.Name,
            Type = entity.Type,
            TotalMentions = -1 //TODO: Implement total mentions calculation
        };

        return Ok(dto);
    }

    /// <summary>
    /// Returns a timeline of mentions for a specific entity.
    /// </summary>
    [HttpGet("{entityId}/timeline")]
    public async Task<ActionResult<EntityTimelineResponse>> GetEntityTimelineAsync(
        string entityId,
        [FromQuery] EntityTimelineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _entityRepository.GetEntityByIdAsync(entityId);
        var articles = await _entityRepository.GetMentioningArticlesAsync(entityId,
                                                                          request.StartDate,
                                                                          request.EndDate);


        var timeline = articles
            .GroupBy(a => DateOnly.FromDateTime(a.PublishedTime))
            .Select(g => new EntityTimelinePoint
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(p => p.Date);

        var response = new EntityTimelineResponse
        {
            EntityId = entity.Id,
            Name = entity.Name,
            Timeline = timeline.ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Returns articles that mention a specific entity.
    /// </summary>
    [HttpGet("{entityId}/articles")]
    public async Task<ActionResult<MentioningArticlesResponse>> GetArticlesMentioningEntityAsync(
        string entityId,
        [FromQuery] MentioningArticlesRequest request,
        CancellationToken cancellationToken = default)
    {
        var articles = await _entityRepository.GetMentioningArticlesAsync(
            entityId,
            request.StartDate,
            request.EndDate);

        return Ok(new MentioningArticlesResponse { Articles = articles.ToArticleDto().ToList() });
    }

    /// <summary>
    /// Returns related entities that co-occur with the given entity.
    /// </summary>
    [HttpGet("{entityId}/related")]
    public async Task<ActionResult<RelatedEntityResponse>> GetRelatedEntitiesAsync(
        string entityId,
        [FromQuery] RelatedEntitiesRequest request,
        CancellationToken cancellationToken = default)
    {
        // This assumes your repository will have a CoMention query later
        var articles = (await _entityRepository.GetMentioningArticlesAsync(
            entityId,
            request.StartDate,
            request.EndDate)).ToList();

        // var related = articles
        //     .Where(entity => entity.Id != entityId)
        //     .GroupBy(entity => entity.Id)
        //     .Select(g => new EntityCoMentionDto
        //     {
        //         Id = g.Key,
        //         Name = g.First().Name,
        //         CoMentionCount = g.Count()
        //     })
        //     .OrderByDescending(r => r.CoMentionCount);

        return Ok(new RelatedEntityResponse
        {
            CoMentionedEntities = []
        });
    }
}
