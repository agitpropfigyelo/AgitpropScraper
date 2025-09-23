using System.ComponentModel.DataAnnotations;

namespace Agitprop.Web.Api.DTOs.Requests;

public class GetEntityNetworkRequest
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [Range(1, 100)]
    public int Limit { get; set; } = 10;
}