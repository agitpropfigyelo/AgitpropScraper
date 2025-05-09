using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Core.Factories;

/// <summary>
/// Defines the contract for creating link parser instances for different news sources.
/// </summary>
public interface ILinkParserFactory
{
    /// <summary>
    /// Retrieves a link parser for the specified news source.
    /// </summary>
    /// <param name="siteIn">The news source for which to retrieve a link parser.</param>
    /// <returns>An instance of <see cref="ILinkParser"/>.</returns>
    ILinkParser GetLinkParser(NewsSites siteIn);
}
