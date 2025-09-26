using Agitprop.Web.Api.Models;

namespace Agitprop.Web.Api.DTOs.Responses;

public class RelatedEntityResponse
{
    public List<EntityCoMentionDto> CoMentionedEntities { get; set; }
}
