using Agitprop.Web.Api.Services;

using Microsoft.AspNetCore.Mvc;

using OpenTelemetry.Trace;

namespace Agitprop.Web.Api.Controllers;

[ApiController]
[Route("api/entity")]
public class EntityController : ControllerBase
{
    private readonly EntityService _entityService;
    private readonly ILogger<EntityController> _logger;
    private readonly Tracer _tracer;

    public EntityController(EntityService entityService, ILogger<EntityController> logger, Tracer tracer)
    {
        _entityService = entityService;
        _logger = logger;
        _tracer = tracer;
    }

    [HttpGet("{id}/mentions")]
    public async Task<IActionResult> GetMentions(string id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        using var span = _tracer.StartActiveSpan("GetMentions");
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Missing entity id." });
            var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
            var toDate = to ?? DateTime.UtcNow;
            var mentions = await _entityService.GetMentionsAsync(id, fromDate, toDate);
            return Ok(new { entityId = id, mentions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMentions");
            return StatusCode(500, new { error = "Internal server error." });
        }
    }

    [HttpGet("{id}/trending")]
    public async Task<IActionResult> GetTrending(string id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        using var span = _tracer.StartActiveSpan("GetTrending");
        try
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Missing entity id." });
            var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
            var toDate = to ?? DateTime.UtcNow;
            var entity = await _entityService.GetEntityAsync(id);
            if (entity is null)
                return NotFound(new { error = "Entity not found." });
            var counts = await _entityService.GetTrendingMentionsAsync(id, fromDate, toDate);
            return Ok(new
            {
                entityName = entity.Name,
                mentionCounts = counts.Select(c => new { date = c.date.ToString("yyyy-MM-dd"), count = c.count })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetTrending");
            return StatusCode(500, new { error = "Internal server error." });
        }
    }
}
