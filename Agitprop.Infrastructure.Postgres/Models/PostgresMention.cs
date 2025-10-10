namespace Agitprop.Infrastructure.Postgres.Models
{
    public class PostgresMention
    {
        public Guid Id { get; set; }

        public Guid ArticleId { get; set; }
        public PostgresArticle Article { get; set; } = null!;

        public Guid EntityId { get; set; }
        public PostgresEntity Entity { get; set; } = null!;
    }
}