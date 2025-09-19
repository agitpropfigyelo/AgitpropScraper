using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agitprop.Infrastructure.Postgres.Models;

[Table("mentions")]
public class PostgresMention
{
    [Key] public Guid Id { get; set; }

    public Guid ArticleId { get; set; }
    [ForeignKey(nameof(ArticleId))] public PostgresArticle Article { get; set; } = null!;

    public Guid EntityId { get; set; }
    [ForeignKey(nameof(EntityId))] public PostgresEntity Entity { get; set; } = null!;
}
