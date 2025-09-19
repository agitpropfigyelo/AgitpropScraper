using System;

using Agitprop.Core;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure.Postgres;

public class NewsfeedDB : INewsfeedDB
{
    public Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsUrlAlreadyExists(string url)
    {
        throw new NotImplementedException();
    }
}
