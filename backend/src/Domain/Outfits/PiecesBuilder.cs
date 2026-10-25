using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.Outfits;

public static class PiecesBuilder
{
    public static OutfitPieces FromIds(IEnumerable<string> ids, IReadOnlyList<Item> closet)
    {
        string? outer = null;
        string top = "", bottom = "", shoes = "";
        var accessories = new List<string>();

        foreach (var id in ids)
        {
            var item = closet.FirstOrDefault(candidate => candidate.Id == id)
                ?? throw new EntityNotFoundException("Item", id);
            switch (item.Slot)
            {
                case Slot.Outer: outer = item.Id; break;
                case Slot.Top: top = item.Id; break;
                case Slot.Bottom: bottom = item.Id; break;
                case Slot.Shoes: shoes = item.Id; break;
                default: accessories.Add(item.Id); break;
            }
        }

        return new OutfitPieces { Outer = outer, Top = top, Bottom = bottom, Shoes = shoes, Acc = accessories };
    }
}
