namespace Agitprop.Web.Api.DTOs.Requests;

public class RelatedEntitiesRequest
{
    public string EntityId { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
