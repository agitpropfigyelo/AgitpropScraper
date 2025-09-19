using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agitprop.Infrastructure.Postgres.Models;

[Table("articles")]
public class PostgresArticle
{
    [Key] public Guid Id { get; set; }
    public DateTime PublishedTime { get; set; }
    public string Url { get; set; } = string.Empty;
    public ICollection<PostgresMention> Mentions { get; set; }
}
