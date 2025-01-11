using SurrealDb.Net.Models;

namespace Agitporp.Scraper.Sinks.Newsfeed.Database.Models;

internal class Entity : Record
{
    public string Name { get; set; }
}
