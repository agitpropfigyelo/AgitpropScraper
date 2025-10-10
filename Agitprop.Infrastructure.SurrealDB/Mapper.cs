using Agitprop.Core.Models;
using Agitprop.Infrastructure.SurrealDB.Models;

namespace Agitprop.Infrastructure.SurrealDB;

internal static class Mapper
{

    public static Entity ToEnity(this EntityRecord record)
    {
        return new Entity
        {
            Id = record.Id.DeserializeId<string>() ?? string.Empty,
            Name = record.Name ?? string.Empty,
        };
    }

    public static Article ToArticle(this ArticleRecord record)
    {
        return new Article
        {
            Url = record.Url ?? string.Empty,
            PublishedTime = record.PublishedTime,
        };
    }
}
