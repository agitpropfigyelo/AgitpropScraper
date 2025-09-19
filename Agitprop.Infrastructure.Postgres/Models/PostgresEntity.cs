using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agitprop.Infrastructure.Postgres.Models;

[Table("entity")]
public class PostgresEntity
{
    [Key] public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<PostgresMention> Mentions { get; set; }
}
