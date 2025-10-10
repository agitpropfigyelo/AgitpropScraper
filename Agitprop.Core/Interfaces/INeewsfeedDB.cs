namespace Agitprop.Core.Interfaces;

public interface INewsfeedDB
{
    public Task<int> CreateMentionsAsync(string url, ContentParserResult article, NamedEntityCollection entities);

    public Task<bool> IsUrlAlreadyExists(string url);

}
