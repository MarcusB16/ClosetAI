using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;

namespace ClosetAPI.Domain.Wears;

public static class WearRecorder
{
    public static void Apply(DateOnly date, Outfit outfit, Outfit? replacedOutfit, IReadOnlyList<Item> closet)
    {
        if (replacedOutfit is not null) UndoWear(replacedOutfit, closet);

        foreach (var item in ItemsOf(outfit, closet))
        {
            item.TimesWorn += 1;
            item.LastWorn = date;
        }
    }

    public static void MarkLastWorn(DateOnly date, Outfit outfit, IReadOnlyList<Item> closet)
    {
        foreach (var item in ItemsOf(outfit, closet))
        {
            if (item.LastWorn is null || date > item.LastWorn) item.LastWorn = date;
        }
    }

    private static void UndoWear(Outfit outfit, IReadOnlyList<Item> closet)
    {
        foreach (var item in ItemsOf(outfit, closet))
            item.TimesWorn = Math.Max(0, item.TimesWorn - 1);
    }

    private static IEnumerable<Item> ItemsOf(Outfit outfit, IReadOnlyList<Item> closet) =>
        outfit.Pieces.AllIds().Select(id => closet.FirstOrDefault(item => item.Id == id)).OfType<Item>();
}
