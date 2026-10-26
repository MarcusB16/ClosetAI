using System.Text.Json;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;

namespace ClosetAPI.Infrastructure.Ai;

public static class OutfitResponseReader
{
    public static StyledOutfit ReadOutfit(JsonElement json, IReadOnlyList<Item> closet, string? requiredItemId)
    {
        var pieces = new OutfitPieces
        {
            Outer = ReadOuter(json, closet),
            Top = ReadSlot(json, "top", Slot.Top, closet),
            Bottom = ReadSlot(json, "bottom", Slot.Bottom, closet),
            Shoes = ReadSlot(json, "shoes", Slot.Shoes, closet),
            Acc = ReadAccessories(json, closet),
        };

        if (requiredItemId is not null && !pieces.AllItemIds().Contains(requiredItemId))
            throw new AiProviderException("The AI response ignored the locked item.");

        return new StyledOutfit(json.RequiredString("title"), json.RequiredString("note"), pieces);
    }

    private static string? ReadOuter(JsonElement json, IReadOnlyList<Item> closet)
    {
        var id = json.RequiredString("outer");
        return id == OutfitSchema.NoOuterValue ? null : ValidatedId(id, Slot.Outer, closet);
    }

    private static string ReadSlot(JsonElement json, string property, Slot slot, IReadOnlyList<Item> closet) =>
        ValidatedId(json.RequiredString(property), slot, closet);

    private static List<string> ReadAccessories(JsonElement json, IReadOnlyList<Item> closet)
    {
        if (!json.TryGetProperty("acc", out var array) || array.ValueKind != JsonValueKind.Array)
            throw new AiProviderException("The AI response is missing 'acc'.");

        return array.EnumerateArray()
            .Select(element => element.GetString() ?? "")
            .Where(id => id != OutfitSchema.NoOuterValue)
            .Select(id => ValidatedId(id, Slot.Acc, closet))
            .Distinct()
            .ToList();
    }

    private static string ValidatedId(string id, Slot slot, IReadOnlyList<Item> closet)
    {
        var item = closet.FirstOrDefault(candidate => candidate.Id == id)
            ?? throw new AiProviderException($"The AI response used unknown item '{id}'.");
        if (item.Slot != slot)
            throw new AiProviderException($"The AI response put item '{id}' in the wrong slot.");
        return id;
    }
}
