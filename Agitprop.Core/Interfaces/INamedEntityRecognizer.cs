namespace Agitprop.Core.Interfaces;

public interface INamedEntityRecognizer
{
    public Task<NamedEntityCollection> AnalyzeSingleAsync(object corpus);
    public Task<NamedEntityCollection[]> AnalyzeBatchAsync(object[] corpora);

}
