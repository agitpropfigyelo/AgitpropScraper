using SurrealDb.Net.Models;

namespace Agitporp.Scraper.Sinks.Newsfeed.Database.Models
{
    internal class Source : Record
    {
        public string Src { get; set; }
    }
}
