using Agitprop.Core;
using Agitprop.Infrastructure;

namespace Agitprop.Scraper.Sinks.Newsfeed.Interfaces;

public interface INewsfeedDB
{
    public Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities);

    public Task<bool> IsUrlAlreadyExists(string url);

}
