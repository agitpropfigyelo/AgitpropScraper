namespace Agitprop.Core.Interfaces;

/// <summary>
/// Defines the contract for a named entity recognizer service.
/// </summary>
public interface INamedEntityRecognizer
{
    /// <summary>
    /// Analyzes a single text corpus for named entities.
    /// </summary>
    /// <param name="corpus">The text corpus to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the named entity collection.</returns>
    public Task<NamedEntityCollection> AnalyzeSingleAsync(string corpus);

    /// <summary>
    /// Analyzes a batch of text corpora for named entities.
    /// </summary>
    /// <param name="corpora">The array of text corpora to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an array of named entity collections.</returns>
    public Task<NamedEntityCollection[]> AnalyzeBatchAsync(string[] corpora);
}
