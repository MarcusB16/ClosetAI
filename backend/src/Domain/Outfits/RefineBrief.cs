using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.Outfits;

public record RefineBrief(
    Outfit Current,
    string Instruction,
    IReadOnlyList<Item> Closet,
    StyleContext Context,
    int Variation);
