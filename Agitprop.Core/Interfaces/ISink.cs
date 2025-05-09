using System.Text.Json.Nodes;

using Agitprop.Core;

namespace Agitprop.Infrastructure.Interfaces;

/// <summary>
/// Defines the contract for a sink that processes and stores scraped data.
/// </summary>
public interface ISink
{
    /// <summary>
    /// Processes and stores the parsed content data for a specific URL.
    /// </summary>
    /// <param name="url">The URL of the page being processed.</param>
    /// <param name="data">The list of parsed content results.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task EmitAsync(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a page with the specified URL has already been visited.
    /// </summary>
    /// <param name="url">The URL of the page to check.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the page has been visited.</returns>
    Task<bool> CheckPageAlreadyVisited(string url);
}
