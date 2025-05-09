using SurrealDb.Net.Models;

namespace Agitprop.Scraper.Sinks.Newsfeed.Database.Models
{
    /// <summary>
    /// Represents a source entity in the database.
    /// </summary>
    internal class Source : Record
    {
        /// <summary>
        /// Gets or sets the source identifier.
        /// </summary>
        public required string Src { get; set; }
    }
}
