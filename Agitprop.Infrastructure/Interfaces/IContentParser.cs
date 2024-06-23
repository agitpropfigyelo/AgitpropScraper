using System.Text.Json.Nodes;
using HtmlAgilityPack;

namespace Agitprop.Infrastructure;

public interface IContentParser
{
    //TODO ennek valami jobb signatura meg aggregalasi logika
    Task<(string,object)> ParseContentAsync(HtmlDocument html);
    Task<(string,object)> ParseContentAsync(string html);
}
