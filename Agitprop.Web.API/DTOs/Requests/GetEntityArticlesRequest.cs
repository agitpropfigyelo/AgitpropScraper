using System.ComponentModel.DataAnnotations;

namespace Agitprop.Web.Api.DTOs.Requests;

public class GetEntityArticlesRequest
{
    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 50;
}