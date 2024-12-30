namespace Agitporp.Scraper.Sinks.Newsfeed.Interfaces;

public interface INamedEntityRecognizer
{
    public Task<NamedEntityCollection> AnalyzeSingleAsync(object corpus);
    public Task<NamedEntityCollection[]> AnalyzeBatchAsync(object[] corpora);

}
