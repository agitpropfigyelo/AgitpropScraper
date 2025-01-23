using SurrealDb.Net.Models;

namespace Agitprop.Scraper.Sinks.Newsfeed.Database.Models;

internal class Mentions : RelationRecord
{

    public string Url { get; set; }
    public DateTime Date { get; set; }
}
