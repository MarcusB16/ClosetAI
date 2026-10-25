using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.Outfits;

public static class OutfitFormality
{
    private const double UnknownItemFormality = Formality.Smart;

    public static double Average(OutfitPieces pieces, IReadOnlyList<Item> closet)
    {
        var ids = new[] { pieces.Outer, pieces.Top, pieces.Bottom, pieces.Shoes }
            .Where(id => !string.IsNullOrEmpty(id));
        var values = ids
            .Select(id => closet.FirstOrDefault(item => item.Id == id)?.Formality ?? UnknownItemFormality)
            .ToList();
        return values.Count == 0 ? UnknownItemFormality : values.Average();
    }
}
