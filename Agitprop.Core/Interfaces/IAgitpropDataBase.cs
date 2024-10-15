using Agitprop.Core.Enums;

namespace Agitprop.Core.Interfaces;

public interface IAgitpropDataBaseService
{
    Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities);
}