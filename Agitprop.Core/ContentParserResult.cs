using System.Text.Json.Serialization;

using Agitprop.Core.Enums;

namespace Agitprop.Core;

/// <summary>
/// Represents the result of parsing content from a web page.
/// </summary>
public record class ContentParserResult
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required NewsSites SourceSite { get; init; }

    public required DateTimeOffset PublishDate { get; init; }
    public required string Text { get; init; }
}
