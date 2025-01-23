using SurrealDb.Net.Models;

namespace Agitprop.Scraper.Sinks.Newsfeed.Database.Models
{
    internal class Source : Record
    {
        public string Src { get; set; }
    }
}
