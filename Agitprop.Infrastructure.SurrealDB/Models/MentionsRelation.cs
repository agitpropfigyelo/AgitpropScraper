using SurrealDb.Net.Models;

namespace Agitprop.Infrastructure.SurrealDB.Models;

/// <summary>
/// Represents a relationship indicating that an article mentions an entity.
/// </summary>
public class MentionsRelation : RelationRecord
{
    /// <summary>
    /// Gets or sets the URL of the article that mentions the entity.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the date when the mention occurred.
    /// </summary>
    public DateTimeOffset Date { get; set; }
}
