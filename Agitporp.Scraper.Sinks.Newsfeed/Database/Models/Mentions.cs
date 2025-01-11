using SurrealDb.Net.Models;

namespace Agitporp.Scraper.Sinks.Newsfeed.Database.Models;

internal class Mentions : RelationRecord
{

    public string Url { get; set; }
    public DateTime Date { get; set; }
}
