using SurrealDb.Net.Models;

namespace Agitprop.Scraper.Sinks.Newsfeed.Database.Models;

internal class VisitedLink : Record
{
    public string Link { get; set; }
}
