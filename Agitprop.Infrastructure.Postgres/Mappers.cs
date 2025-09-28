using System;

using Agitprop.Core.Models;
using Agitprop.Infrastructure.Postgres.Models;

namespace Agitprop.Infrastructure.Postgres;

public static class Mappers
{
    public static Entity ToCoreModel(this PostgresEntity entity)
    {
        return new Entity
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
        };
    }

    public static IEnumerable<Entity> ToCoreModel(this IEnumerable<PostgresEntity> entities)
    {
        foreach (var entity in entities)
        {
            yield return entity.ToCoreModel();
        }
    }

    public static Article ToCoreModel(this PostgresArticle article)
    {
        return new Article
        {
            Id = article.Id.ToString(),
            Title = article.Title,
            Url = article.Url,
            PublishedTime = article.PublishedTime,
        };
    }

    public static IEnumerable<Article> ToCoreModel(this IEnumerable<PostgresArticle> articles)
    {
        foreach (var article in articles)
        {
            yield return article.ToCoreModel();
        }
    }
}
