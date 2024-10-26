using System.Text.Json.Nodes;
using Agitprop.Core;

namespace Agitprop.Infrastructure.Interfaces;

public interface ISink
{
    Task EmitAsync(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default);
    void Emit(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default);
}