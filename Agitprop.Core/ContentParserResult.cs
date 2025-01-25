using System.Text.Json.Serialization;

using Agitprop.Core.Enums;

namespace Agitprop.Core;

public record class ContentParserResult
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required NewsSites SourceSite { get; init; }

    public required DateTime PublishDate { get; init; }
    public required string Text { get; init; }
}
