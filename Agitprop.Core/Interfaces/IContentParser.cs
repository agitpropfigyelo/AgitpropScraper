using HtmlAgilityPack;

namespace Agitprop.Core.Interfaces;

public interface IContentParser
{
    //TODO ennek valami jobb signatura meg aggregalasi logika
    Task<ContentParserResult> ParseContentAsync(HtmlDocument html);
    Task<ContentParserResult> ParseContentAsync(string html);
}
