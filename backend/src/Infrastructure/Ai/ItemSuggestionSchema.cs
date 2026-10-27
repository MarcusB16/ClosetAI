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
            ["name"] = TextProperty(),
            ["brand"] = TextProperty(),
            ["category"] = ChoiceProperty(Enum.GetNames<Category>().Select(CamelCase)),
            ["slot"] = ChoiceProperty(Enum.GetNames<Slot>().Select(CamelCase)),
            ["type"] = ChoiceProperty(GarmentTypes.All),
            ["colorName"] = TextProperty(),
            ["color"] = TextProperty(),
            ["formality"] = new JsonObject
            {
                ["type"] = "integer",
                ["enum"] = new JsonArray(Formality.Casual, Formality.Smart, Formality.Formal),
            },
            ["seasons"] = new JsonObject { ["type"] = "array", ["items"] = ChoiceProperty(SeasonNames) },
            ["fabric"] = TextProperty(),
        },
    };

    public static string CamelCase(string name) => char.ToLowerInvariant(name[0]) + name[1..];

    private static JsonObject TextProperty() => new() { ["type"] = "string" };

    private static JsonObject ChoiceProperty(IEnumerable<string> values) => new()
    {
        ["type"] = "string",
        ["enum"] = new JsonArray(values.Select(value => (JsonNode)JsonValue.Create(value)!).ToArray()),
    };
}
