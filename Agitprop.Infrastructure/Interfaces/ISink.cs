using System.Text.Json.Nodes;

namespace Agitprop.Infrastructure.Interfaces;

public interface ISink
{
    Task EmitAsync(string url, Dictionary<string, object> data, CancellationToken cancellationToken = default);
}