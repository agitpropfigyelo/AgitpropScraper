using SurrealDb.Net.Models;

namespace Agitprop.Infrastructure.SurrealDB.Models
{
    internal class Mentions : Record
    {
        public Source @in { get; set; }
        public Entity @out { get; set; }
        public string Url { get; set; }
        public string Date { get; set; }
    }
}
