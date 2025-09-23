using System.ComponentModel.DataAnnotations;

namespace Agitprop.Web.Api.DTOs.Requests;

public class GetEntityDetailsRequest
{
    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }
}