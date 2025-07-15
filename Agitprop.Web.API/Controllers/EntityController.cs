using Agitprop.Web.Api.Services;

using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;

[ApiController]
[Route("api/[controller]")]
public class EntitiesController : ControllerBase
{
    private readonly EntityService _entityService;
    private readonly ILogger<EntitiesController> _logger;

    public EntitiesController(EntityService entityService, ILogger<EntitiesController> logger)
    {
        _entityService = entityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<EntityDto>>> GetEntities(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        using var activity = Activity.Current?.Source.StartActivity("GetEntities");

        if (page < 1)
        {
            _logger.LogWarning("Invalid page parameter: {Page}", page);
            return BadRequest("Page must be greater than 0.");
        }
        if (pageSize < 1 || pageSize > 200)
        {
            _logger.LogWarning("Invalid pageSize parameter: {PageSize}", pageSize);
            return BadRequest("PageSize must be between 1 and 200.");
        }

        if (!startDate.HasValue)
        {
            _logger.LogWarning("StartDate is required.");
            return BadRequest("StartDate is required.");
        }
        if (!endDate.HasValue)
        {
            _logger.LogWarning("EndDate is required.");
            return BadRequest("EndDate is required.");
        }

        if (startDate > endDate)
        {
            _logger.LogWarning("StartDate after EndDate: startDate={StartDate}, endDate={EndDate}", startDate, endDate);
            return BadRequest("StartDate cannot be after EndDate.");
        }


        _logger.LogInformation("Fetching entities: startDate={StartDate}, endDate={EndDate}",
            startDate, endDate);

        var result = await _entityService.GetEntitiesAsync(startDate.Value, endDate.Value, page, pageSize);

        _logger.LogInformation("Entities fetched: count={Count}", result.Count);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EntityDetailsDto>> GetEntity(
        Guid id,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate)
    {
        using var activity = Activity.Current?.Source.StartActivity("GetEntityDetails");
        //TODO: na ez biztos nem így lesz, mert nem ilyen a surrealDB guidja
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid entity id: {EntityId}", id);
            return BadRequest("Invalid entity id.");
        }

        if (!startDate.HasValue)
        {
            _logger.LogWarning("StartDate is required.");
            return BadRequest("StartDate is required.");
        }
        if (!endDate.HasValue)
        {
            _logger.LogWarning("EndDate is required.");
            return BadRequest("EndDate is required.");
        }

        if (startDate > endDate)
        {
            _logger.LogWarning("StartDate after EndDate: startDate={StartDate}, endDate={EndDate}", startDate, endDate);
            return BadRequest("StartDate cannot be after EndDate.");
        }

        _logger.LogInformation("Fetching entity details: id={EntityId}, startDate={StartDate}, endDate={EndDate}",
            id, startDate, endDate);

        var result = await _entityService.GetEntityDetailsAsync(id, startDate.Value, endDate.Value);
        if (result == null)
        {
            _logger.LogWarning("Entity not found: id={EntityId}", id);
            return NotFound();
        }

        _logger.LogInformation("Entity details fetched: id={EntityId}", id);

        return Ok(result);
    }

    // A többi endpoint is hasonló Activity+Log pattern-nel

    [HttpGet("{id}/articles")]
    public async Task<ActionResult<List<ArticleDto>>> GetEntityMentioningArticles(
        Guid id, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        using var activity = Activity.Current?.Source.StartActivity("GetEntityArticles");

        // Input validation
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid entity id: {EntityId}", id);
            return BadRequest("Invalid entity id.");
        }
        if (page < 1)
        {
            _logger.LogWarning("Invalid page parameter: {Page}", page);
            return BadRequest("Page must be greater than 0.");
        }
        if (pageSize < 1 || pageSize > 200)
        {
            _logger.LogWarning("Invalid pageSize parameter: {PageSize}", pageSize);
            return BadRequest("PageSize must be between 1 and 200.");
        }
        if (!startDate.HasValue)
        {
            _logger.LogWarning("StartDate is required.");
            return BadRequest("StartDate is required.");
        }
        if (!endDate.HasValue)
        {
            _logger.LogWarning("EndDate is required.");
            return BadRequest("EndDate is required.");
        }

        if (startDate > endDate)
        {
            _logger.LogWarning("StartDate after EndDate: startDate={StartDate}, endDate={EndDate}", startDate, endDate);
            return BadRequest("StartDate cannot be after EndDate.");
        }

        _logger.LogInformation("Fetching articles for entity: id={EntityId}, startDate={StartDate}, endDate={EndDate}, page={Page}, pageSize={PageSize}",
            id, startDate, endDate, page, pageSize);

        var result = await _entityService.GetMentioningArticlesAsync(id, startDate.Value, endDate.Value, page, pageSize);
        _logger.LogInformation("Articles fetched: count={Count}", result.Count);

        return Ok(result);
    }

    [HttpGet("{id}/network")]
    public async Task<ActionResult<List<NetworkItemDto>>> GetEntityNetwork(
        Guid id, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] int limit = 10)
    {
        using var activity = Activity.Current?.Source.StartActivity("GetEntityNetwork");

        // Input validation
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid entity id: {EntityId}", id);
            return BadRequest("Invalid entity id.");
        }
        if (limit < 1 || limit > 100)
        {
            _logger.LogWarning("Invalid limit parameter: {Limit}", limit);
            return BadRequest("Limit must be between 1 and 100.");
        }
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            _logger.LogWarning("StartDate after EndDate: startDate={StartDate}, endDate={EndDate}", startDate, endDate);
            return BadRequest("StartDate cannot be after EndDate.");
        }
        _logger.LogInformation("Fetching network for entity: id={EntityId}, startDate={StartDate}, endDate={EndDate}, limit={Limit}",
            id, startDate, endDate, limit);

        var result = await _entityService.GetNetworkDetailskAsync(id, startDate, endDate, limit);

        _logger.LogInformation("Network fetched: count={Count}", result.Count);

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<EntityDto>>> SearchEntities([FromQuery] string query)
    {
        using var activity = Activity.Current?.Source.StartActivity("SearchEntities");


        // Input validation
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2 || query.Length > 100)
        {
            _logger.LogWarning("Invalid search query: {Query}", query);
            return BadRequest("Query parameter is required and must be between 2 and 100 characters.");
        }

        _logger.LogInformation("Searching entities: query={Query}", query);

        var result = await _entityService.SearchForEntity(query);

        _logger.LogInformation("Search result: count={Count}", result.Count);

        return Ok(result);
    }
}
