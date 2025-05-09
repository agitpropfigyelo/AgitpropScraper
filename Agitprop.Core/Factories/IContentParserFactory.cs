using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Core.Factories
{
    /// <summary>
    /// Defines the contract for creating content parser instances for different news sources.
    /// </summary>
    public interface IContentParserFactory
    {
        /// <summary>
        /// Retrieves a content parser for the specified news source.
        /// </summary>
        /// <param name="siteIn">The news source for which to retrieve a content parser.</param>
        /// <returns>An instance of <see cref="IContentParser"/>.</returns>
        IContentParser GetContentParser(NewsSites siteIn);
    }
}
