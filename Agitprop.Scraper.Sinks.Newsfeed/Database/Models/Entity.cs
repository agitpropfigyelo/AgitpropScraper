using SurrealDb.Net.Models;

namespace Agitprop.Scraper.Sinks.Newsfeed.Database.Models;

internal class Entity : Record
{
    public string Name { get; set; }
}
