using System.ComponentModel.DataAnnotations;

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

    public EntityController(EntityService entityService, ILogger<EntityController> logger)
    {
        _entityService = entityService;
        _logger = logger;
    }
    [HttpGet]
    public async Task<IActionResult> GetEntities([FromQuery][MaxLength(100)] string? query)
    {
        var result = await _entityService.GetEntitiesAsync(query);
        return Ok(result);
    }

    [HttpGet("{id}/mentions")]
    public async Task<IActionResult> GetMentions(string id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { error = "Missing entity id." });

        var mentioningArticles = await _entityService.GetMentioningArticlesAsync(id, from, to);
        return Ok(new { entityId = id, mentioningArticles });
    }
}
