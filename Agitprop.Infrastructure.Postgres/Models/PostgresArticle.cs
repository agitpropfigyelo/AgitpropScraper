namespace Agitprop.Infrastructure.Postgres.Models;

public class PostgresArticle
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedTime { get; set; }

    public ICollection<PostgresMention> Mentions { get; set; } = new List<PostgresMention>();
}
