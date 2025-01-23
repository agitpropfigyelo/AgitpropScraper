using Agitprop.Core.Enums;

namespace Agitprop.Core;

[Serializable]
public record class ContentParserResult
{
    public required NewsSites SourceSite;
    public required DateTime PublishDate;
    public required string Text;
}
