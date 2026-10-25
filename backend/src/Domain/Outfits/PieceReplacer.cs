using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.Outfits;

public static class PieceReplacer
{
    public static OutfitPieces Replace(OutfitPieces pieces, Slot slot, Item replacement, IReadOnlyList<Item> closet)
    {
        if (replacement.Slot != slot)
            throw new RuleViolationException("itemId", $"Item '{replacement.Id}' belongs to slot '{replacement.Slot}', not '{slot}'.");

        return slot switch
        {
            Slot.Outer => pieces with { Outer = replacement.Id },
            Slot.Top => pieces with { Top = replacement.Id },
            Slot.Bottom => pieces with { Bottom = replacement.Id },
            Slot.Shoes => pieces with { Shoes = replacement.Id },
            _ => pieces with { Acc = ReplaceAccessory(pieces.Acc, replacement, closet) },
        };
    }

    private static List<string> ReplaceAccessory(List<string> current, Item replacement, IReadOnlyList<Item> closet)
    {
        var accessories = current.Where(id => id != replacement.Id).ToList();
        var sameKindIndex = accessories.FindIndex(id => IsWatch(id, closet) == replacement.IsWatch);
        if (sameKindIndex >= 0) accessories[sameKindIndex] = replacement.Id;
        else accessories.Add(replacement.Id);
        return accessories;
    }

    private static bool IsWatch(string id, IReadOnlyList<Item> closet) =>
        closet.FirstOrDefault(item => item.Id == id)?.IsWatch == true;
}
