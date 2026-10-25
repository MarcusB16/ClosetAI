using System.Text.Json.Nodes;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Infrastructure.Ai;

public static class OutfitSchema
{
    public const string NoOuterValue = "none";

    public static JsonObject For(IReadOnlyList<Item> closet) => new()
    {
        ["type"] = "object",
        ["additionalProperties"] = false,
        ["required"] = new JsonArray("title", "note", "outer", "top", "bottom", "shoes", "acc"),
        ["properties"] = new JsonObject
        {
            ["title"] = new JsonObject { ["type"] = "string" },
            ["note"] = new JsonObject { ["type"] = "string" },
            ["outer"] = IdChoice(closet, Slot.Outer, includeNone: true),
            ["top"] = IdChoice(closet, Slot.Top, includeNone: false),
            ["bottom"] = IdChoice(closet, Slot.Bottom, includeNone: false),
            ["shoes"] = IdChoice(closet, Slot.Shoes, includeNone: false),
            ["acc"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = IdChoice(closet, Slot.Acc, includeNone: true),
            },
        },
    };

    private static JsonObject IdChoice(IReadOnlyList<Item> closet, Slot slot, bool includeNone)
    {
        var ids = closet.Where(item => item.Slot == slot).Select(item => item.Id).ToList();
        if (includeNone) ids.Add(NoOuterValue);
        return new JsonObject
        {
            ["type"] = "string",
            ["enum"] = new JsonArray(ids.Select(id => (JsonNode)JsonValue.Create(id)!).ToArray()),
        };
    }
}
