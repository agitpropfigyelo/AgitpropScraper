using SurrealDb.Net.Models;

namespace Agitporp.Scraper.Sinks.Newsfeed.Database.Models;

internal class VisitedLink : Record
{
    public string Link { get; set; }
}
