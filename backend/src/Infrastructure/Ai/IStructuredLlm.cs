using System.Text.Json;

namespace ClosetAPI.Infrastructure.Ai;

public interface IStructuredLlm
{
    Task<JsonElement> CompleteAsync(StructuredRequest request, CancellationToken cancellationToken);
}
