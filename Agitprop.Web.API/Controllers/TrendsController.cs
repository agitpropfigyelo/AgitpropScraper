using Agitprop.Core.Interfaces;

using Microsoft.AspNetCore.Mvc;

using OpenTelemetry.Trace;

namespace Agitprop.Web.Api.Controllers;

[ApiController]
[Route("api/trending")]
public class TrendsController : ControllerBase
{
    private readonly ITrendingRepository _trendingRepository;
    private readonly IEntityRepository _entityRepository;
    private readonly ILogger<TrendsController> _logger;

    public TrendsController(ITrendingRepository trendingRepository, ILogger<TrendsController> logger, IEntityRepository entityRepository)
    {
        _trendingRepository = trendingRepository;
        _logger = logger;
        _entityRepository = entityRepository;
    }

    [HttpGet]
    public async Task<ActionResult<TrendingResponse>> GetTrending([FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        try
        {
            var trending = _trendingRepository.GetTrendingEntitiesAsync(from, to);
            var mentionings = _entityRepository.GetMentioningArticlesAsync(trending.Select(e => e.Id.ToString()), from, to);
            
            var result=trending.Select(e=>new EntityDetailsDto
            {
                Id=e.Id,
                Name=e.Name,
                TotalMentions=mentionings[e.Id.ToString()].Count(),
                MentionsCountByDate=mentionings[e.Id.ToString()].GroupBy(a=>DateOnly.FromDateTime(a.PublishedTime)).ToDictionary(g=>g.Key,g=>g.Count())
            });
            return Ok(new { trending = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTrendingEntities");
            return StatusCode(500, new { error = "Internal server error." });
        }
    }
}

public class TrendingResponse
{
    public required IEnumerable<EntityDetailsDto> Trending { get; set; }
}