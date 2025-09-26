namespace Agitprop.Web.Api.DTOs.Requests;

public class EntityTimelineRequest
{
    public string EntityId { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}
