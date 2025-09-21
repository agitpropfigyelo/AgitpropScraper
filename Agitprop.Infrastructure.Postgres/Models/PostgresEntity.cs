namespace Agitprop.Infrastructure.Postgres.Models;
public class PostgresEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<PostgresMention> Mentions { get; set; } = new List<PostgresMention>();
}
