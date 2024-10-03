using SurrealDb.Net.Models;

namespace Agitprop.Infrastructure.SurrealDB.Models
{
    internal class VisitedLink : Record
    {
        public string Link { get; set; }
    }
}
