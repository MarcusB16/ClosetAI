using System.Text.Json.Nodes;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Infrastructure.Ai;

public static class ItemSuggestionSchema
{
    public static readonly string[] SeasonNames = ["Spring", "Summer", "Fall", "Winter"];

    public static JsonObject Build() => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["required"] = new JsonArray("name", "brand", "category", "slot", "type", "colorName", "color", "formality", "seasons", "fabric"),
        ["properties"] = new JsonObject
        {
            ["name"] = Text(),
            ["brand"] = Text(),
            ["category"] = Choice(Enum.GetNames<Category>().Select(CamelCase)),
            ["slot"] = Choice(Enum.GetNames<Slot>().Select(CamelCase)),
            ["type"] = Choice(GarmentTypes.All),
            ["colorName"] = Text(),
            ["color"] = Text(),
            ["formality"] = new JsonObject
            {
                ["type"] = "integer",
                ["enum"] = new JsonArray(Formality.Casual, Formality.Smart, Formality.Formal),
            },
            ["seasons"] = new JsonObject { ["type"] = "array", ["items"] = Choice(SeasonNames) },
            ["fabric"] = Text(),
        },
    };

    public static string CamelCase(string name) => char.ToLowerInvariant(name[0]) + name[1..];

    private static JsonObject Text() => new() { ["type"] = "string" };

    private static JsonObject Choice(IEnumerable<string> values) => new()
    {
        ["type"] = "string",
        ["enum"] = new JsonArray(values.Select(value => (JsonNode)JsonValue.Create(value)!).ToArray()),
    };
}
