using Agitprop.Web.Api.Services;
using Agitprop.Web.Api.DTOs.Requests;
using Agitprop.Web.Api.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Agitprop.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntitiesController : ControllerBase
    {
    private readonly IEntityService _entityService;
    private readonly ILogger<EntitiesController> _logger;

    public EntitiesController(IEntityService entityService, ILogger<EntitiesController> logger)
    {
        _entityService = entityService;
        _logger = logger;
    }        [HttpPost("search")]
        public async Task<ActionResult<List<EntityResponse>>> GetEntities([FromBody] GetEntitiesRequest request)
        {
            using var activity = Activity.Current?.Source.StartActivity("GetEntities");

            if (request.StartDate > request.EndDate)
            {
                _logger.LogWarning("StartDate after EndDate: startDate={StartDate}, endDate={EndDate}", 
                    request.StartDate, request.EndDate);
                return BadRequest("StartDate cannot be after EndDate.");
            }

            _logger.LogInformation("Fetching entities: startDate={StartDate}, endDate={EndDate}",
                request.StartDate, request.EndDate);

            var result = await _entityService.GetEntitiesAsync(request.StartDate, request.EndDate, request.Page, request.PageSize);

            _logger.LogInformation("Entities fetched: count={Count}", result.Count);

            return Ok(result);
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<EntityDetailsResponse>> GetEntity(Guid id, [FromBody] GetEntityDetailsRequest request)
        {
            using var activity = Activity.Current?.Source.StartActivity("GetEntityDetails");
            
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Invalid entity id: {EntityId}", id);
                return BadRequest("Invalid entity id.");
            }

            if (request.StartDate > request.EndDate)
            {
                _logger.LogWarning("StartDate after EndDate: startDate={StartDate}, endDate={EndDate}", 
                    request.StartDate, request.EndDate);
                return BadRequest("StartDate cannot be after EndDate.");
            }

            _logger.LogInformation("Fetching entity details: id={EntityId}, startDate={StartDate}, endDate={EndDate}",
                id, request.StartDate, request.EndDate);

            var result = await _entityService.GetEntityDetailsAsync(id, request.StartDate, request.EndDate);
            if (result == null)
            {
                _logger.LogWarning("Entity not found: id={EntityId}", id);
                return NotFound();
            }

            _logger.LogInformation("Entity details fetched: id={EntityId}", id);

            return Ok(result);
        }

        [HttpPost("{id}/articles")]
        public async Task<ActionResult<List<ArticleResponse>>> GetEntityMentioningArticles(
            Guid id, [FromBody] GetEntityArticlesRequest request)
        {
            using var activity = Activity.Current?.Source.StartActivity("GetEntityArticles");

            if (id == Guid.Empty)
            {
                _logger.LogWarning("Invalid entity id: {EntityId}", id);
                return BadRequest("Invalid entity id.");
            }

            if (request.StartDate > request.EndDate)
            {
                _logger.LogWarning("StartDate after EndDate: startDate={StartDate}, endDate={EndDate}", 
                    request.StartDate, request.EndDate);
                return BadRequest("StartDate cannot be after EndDate.");
            }

            _logger.LogInformation("Fetching articles for entity: id={EntityId}, startDate={StartDate}, endDate={EndDate}, page={Page}, pageSize={PageSize}",
                id, request.StartDate, request.EndDate, request.Page, request.PageSize);

            var result = await _entityService.GetMentioningArticlesAsync(id, request.StartDate, request.EndDate, request.Page, request.PageSize);
            _logger.LogInformation("Articles fetched: count={Count}", result.Count);

            return Ok(result);
        }

        [HttpPost("{id}/network")]
        public async Task<ActionResult<List<NetworkItemResponse>>> GetEntityNetwork(
            Guid id, [FromBody] GetEntityNetworkRequest request)
        {
            using var activity = Activity.Current?.Source.StartActivity("GetEntityNetwork");

            if (id == Guid.Empty)
            {
                _logger.LogWarning("Invalid entity id: {EntityId}", id);
                return BadRequest("Invalid entity id.");
            }

            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            {
                _logger.LogWarning("StartDate after EndDate: startDate={StartDate}, endDate={EndDate}", 
                    request.StartDate, request.EndDate);
                return BadRequest("StartDate cannot be after EndDate.");
            }

            _logger.LogInformation("Fetching network for entity: id={EntityId}, startDate={StartDate}, endDate={EndDate}, limit={Limit}",
                id, request.StartDate, request.EndDate, request.Limit);

            var result = await _entityService.GetNetworkDetailsAsync(id, request.StartDate, request.EndDate, request.Limit);

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
}