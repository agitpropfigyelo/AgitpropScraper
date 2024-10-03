using SurrealDb.Net.Models;

namespace Agitprop.Infrastructure.SurrealDB.Models
{
    internal class Entity : Record
    {
        public string Name { get; set; }
    }
}
