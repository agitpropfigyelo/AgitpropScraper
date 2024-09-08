using Agitprop.Core;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure;

public class Neo4jProvider : IAgitpropDataBaseService
{
    public Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities)
    {
        throw new NotImplementedException();
    }

    public Task Initialize()
    {
        throw new NotImplementedException();
    }
}
