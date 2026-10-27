using System.Text.RegularExpressions;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.Outfits;

public static partial class SlotInstructionParser
{
    public static Slot? ParseSlot(string instruction)
    {
        var text = instruction.ToLowerInvariant();
        if (ShoesPattern().IsMatch(text)) return Slot.Shoes;
        if (TopPattern().IsMatch(text)) return Slot.Top;
        if (BottomPattern().IsMatch(text)) return Slot.Bottom;
        if (OuterPattern().IsMatch(text)) return Slot.Outer;
        if (AccessoryPattern().IsMatch(text)) return Slot.Acc;
        return null;
    }

    [GeneratedRegex("shoe|sneaker|boot|loafer|derb|footwear")]
    private static partial Regex ShoesPattern();

    [GeneratedRegex("shirt|top|tee|sweater|knit")]
    private static partial Regex TopPattern();

    [GeneratedRegex("pant|trouser|jean|chino|bottom")]
    private static partial Regex BottomPattern();

    [GeneratedRegex("jacket|coat|blazer|outer|layer")]
    private static partial Regex OuterPattern();

    [GeneratedRegex("watch|belt|scarf|accessor|tie|bag|tote")]
    private static partial Regex AccessoryPattern();
}
