using System.Text.Json.Nodes;

using Agitprop.Core;

namespace Agitprop.Infrastructure.Interfaces;

public interface ISink
{
    Task EmitAsync(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default);

    Task<bool> CheckPageAlreadyVisited(string url);
}
