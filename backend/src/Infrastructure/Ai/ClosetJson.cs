using System.Text.Json;
using ClosetAPI.Domain.Errors;

namespace ClosetAPI.Infrastructure.Ai;

public static class ClosetJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string? OptionalString(this JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    public static string RequiredString(this JsonElement element, string name) =>
        element.OptionalString(name) ?? throw new AiProviderException($"The AI response is missing '{name}'.");
}
