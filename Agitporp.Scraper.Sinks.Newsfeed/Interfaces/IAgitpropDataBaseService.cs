using Agitprop.Core;

namespace Agitporp.Scraper.Sinks.Newsfeed.Interfaces;

public interface INewsfeedDB
{
    public Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities);

}