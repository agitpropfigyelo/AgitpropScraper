using System.Text.Json.Serialization;

using Agitprop.Core.Enums;

namespace Agitprop.Core;

/// <summary>
/// Represents the result of parsing content from a web page.
/// </summary>
public record class ContentParserResult
{
    private DateTime publishDate;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required NewsSites SourceSite { get; init; }

    public required DateTime PublishDate 
    { 
        get => publishDate;
        init => publishDate = value.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
    }
    public required string Text { get; init; }
}
