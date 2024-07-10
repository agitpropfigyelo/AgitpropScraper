using Agitprop.Core.Enums;

namespace Agitprop.Core.Interfaces;

public interface IAgitpropDataBaseService
{
    Task CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities);
    Task Initialize();
}