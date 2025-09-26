namespace Agitprop.Web.Api.DTOs.Requests;

public class MentioningArticlesRequest
{
    public string EntityId { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
