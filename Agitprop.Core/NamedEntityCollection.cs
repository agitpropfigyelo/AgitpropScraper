using System.Text.Json.Serialization;

namespace Agitprop.Core;

/// <summary>
/// Represents a named entity and its type.
/// </summary>
public record NamedEntity
{
    /// <summary>
    /// Gets or sets the entity text.
    /// </summary>
    [JsonPropertyName("Item1")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the entity type.
    /// </summary>
    [JsonPropertyName("Item2")]
    public string Type { get; set; } = "";
}

/// <summary>
/// Represents a collection of named entities.
/// </summary>
public record NamedEntityCollection
{
    /// <summary>
    /// Gets or sets the list of entities with their types.
    /// </summary>
    [JsonPropertyName("entities")]
    public List<NamedEntity> Entities { get; set; } = [];

    /// <summary>
    /// Gets the list of person entities.
    /// </summary>
    public List<NamedEntity> PER => Entities.Where(e => e.Type == "PER").ToList();

    /// <summary>
    /// Gets the list of location entities.
    /// </summary>
    public List<NamedEntity> LOC => Entities.Where(e => e.Type == "LOC").ToList();

    /// <summary>
    /// Gets the list of organization entities.
    /// </summary>
    public List<NamedEntity> ORG => Entities.Where(e => e.Type == "ORG").ToList();

    /// <summary>
    /// Gets the list of miscellaneous entities.
    /// </summary>
    public List<NamedEntity> MISC => Entities.Where(e => e.Type == "MISC").ToList();

    /// <summary>
    /// Gets a combined list of all named entities.
    /// </summary>
    public List<NamedEntity> All => Entities;
}
