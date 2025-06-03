using Agitprop.Infrastructure.SurrealDB;

using Microsoft.AspNetCore.Mvc;

using OpenTelemetry.Trace;

namespace Agitprop.Web.Api.Controllers;

[ApiController]
[Route("api/trending")]
public class TrendsController : ControllerBase
{
    private readonly ITrendingRepository _trendingRepository;
    private readonly ILogger<TrendsController> _logger;
    private readonly Tracer _tracer;

    public TrendsController(ITrendingRepository trendingRepository, ILogger<TrendsController> logger, Tracer tracer)
    {
        _trendingRepository = trendingRepository;
        _logger = logger;
        _tracer = tracer;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrending([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        using var span = _tracer.StartActiveSpan("GetTrendingEntities");
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-7);
            var toDate = to ?? DateTime.UtcNow;
            var trending = await _trendingRepository.GetTrendingEntitiesAsync(fromDate, toDate);
            var result = trending.GroupBy(x => x.date)
                .Select(g => new {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    entities = g.Select(e => new { entityId = e.entityId, entityName = e.entityName, count = e.count })
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
