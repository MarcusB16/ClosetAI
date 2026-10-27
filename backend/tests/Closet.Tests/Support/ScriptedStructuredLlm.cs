using System.Text.Json;
using ClosetAPI.Infrastructure.Ai;

namespace Closet.Tests.Support;

public class ScriptedStructuredLlm(string json) : IStructuredLlm
{
    public StructuredRequest? LastRequest { get; private set; }

    public Task<JsonElement> CompleteWithJsonSchemaAsync(StructuredRequest request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(JsonDocument.Parse(json).RootElement.Clone());
    }
}
