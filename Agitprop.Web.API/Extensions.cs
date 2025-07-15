using System;

using Agitprop.Infrastructure.SurrealDB.Models;

namespace Agitprop.Web.Api;

internal static class Extensions
{
    internal static IEnumerable<EntityDto> ToEntityDtos(this IEnumerable<Entity> entities)
    {
        return entities.Select(e => e.ToEntityDto());
    }
    internal static EntityDto ToEntityDto(this Entity e)
    {
        return new EntityDto
        {
            Id = e.Id.ToString(),
            Name = e.Name,
        };
    }
}