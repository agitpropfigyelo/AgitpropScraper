using SurrealDb.Net.Models;

namespace Agitprop.Infrastructure.SurrealDB.Models;

/// <summary>
/// Represents an article stored in the database.
/// </summary>
public class Article : Record
{
    /// <summary>
    /// Gets the URL of the article.
    /// </summary>
    public string Url { init; get; }

    /// <summary>
    /// Gets the publication time of the article.
    /// </summary>
    public DateTime PublishedTime { init; get; }
}
