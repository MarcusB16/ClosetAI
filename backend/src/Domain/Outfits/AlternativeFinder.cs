using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.Outfits;

public static class AlternativeFinder
{
    private const double MaxFormalityDistance = 1.3;

    public static List<Item> FindAlternatives(OutfitPieces pieces, Slot slot, IReadOnlyList<Item> closet, StyleContext context)
    {
        var target = OutfitFormality.CalculateAverage(pieces, closet);
        var currentId = CurrentItemIdInSlot(pieces, slot, closet);
        return closet
            .Where(item => item.Slot == slot
                && item.Id != currentId
                && !item.IsWatch
                && !(context.Rain && item.NoRain == true)
                && Math.Abs(item.Formality - target) <= MaxFormalityDistance)
            .ToList();
    }

    private static string? CurrentItemIdInSlot(OutfitPieces pieces, Slot slot, IReadOnlyList<Item> closet) => slot switch
    {
        Slot.Outer => pieces.Outer,
        Slot.Top => pieces.Top,
        Slot.Bottom => pieces.Bottom,
        Slot.Shoes => pieces.Shoes,
        _ => pieces.Acc.FirstOrDefault(id => closet.FirstOrDefault(item => item.Id == id)?.IsWatch == false),
    };
}
