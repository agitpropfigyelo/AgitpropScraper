using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Core.Factories;

/// <summary>
/// Defines the contract for creating paginator instances for different news sources.
/// </summary>
public interface IPaginatorFactory
{
    /// <summary>
    /// Retrieves a paginator for the specified news source.
    /// </summary>
    /// <param name="source">The news source for which to retrieve a paginator.</param>
    /// <returns>An instance of <see cref="IPaginator"/>.</returns>
    IPaginator GetPaginator(NewsSites source);
}
