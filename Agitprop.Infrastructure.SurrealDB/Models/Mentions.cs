using SurrealDb.Net.Models;

namespace Agitprop.Infrastructure.SurrealDB.Models
{
    internal class Mentions : RelationRecord
    {

        public string Url { get; set; }
        public DateTime Date { get; set; }
    }
}
