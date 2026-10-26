using System.Text.Json;

namespace ClosetAPI.Infrastructure.Ai;

public interface IStructuredLlm
{
    Task<JsonElement> CompleteWithJsonSchemaAsync(StructuredRequest request, CancellationToken cancellationToken);
}
