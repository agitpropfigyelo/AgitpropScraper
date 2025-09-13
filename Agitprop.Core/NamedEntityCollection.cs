using System.Text.Json.Serialization;

namespace Agitprop.Core;

/// <summary>
/// Represents a collection of named entities categorized by type.
/// </summary>
public record NamedEntityCollection
{
    /// <summary>
    /// Gets or sets the list of person entities.
    /// </summary>
    [JsonPropertyName("PER")]
    public List<string> PER { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of location entities.
    /// </summary>
    [JsonPropertyName("LOC")]
    public List<string> LOC { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of organization entities.
    /// </summary>
    [JsonPropertyName("ORG")]
    public List<string> ORG { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of miscellaneous entities.
    /// </summary>
    [JsonPropertyName("MISC")]
    public List<string> MISC { get; set; } = [];

    /// <summary>
    /// Gets a combined list of all named entities.
    /// </summary>
    public List<string> All
    {
        get
        {
            List<string> all = [];
            all.AddRange(PER);
            all.AddRange(LOC);
            all.AddRange(ORG);
            all.AddRange(MISC);
            return all;
        }
    }
}
