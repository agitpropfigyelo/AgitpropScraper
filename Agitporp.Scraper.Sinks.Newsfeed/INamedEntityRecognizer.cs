using Agitprop.Core;

namespace Agitporp.Scraper.Sinks.Newsfeed;

public interface INamedEntityRecognizer
{
    public Task<NamedEntityCollection> AnalyzeSingleAsync(object corpus);
    public Task<NamedEntityCollection[]> AnalyzeBatchAsync(object[] corpora);

}
