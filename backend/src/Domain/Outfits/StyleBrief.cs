using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.Outfits;

public record StyleBrief(
    string Prompt,
    string? LockItemId,
    bool AvoidRecent,
    IReadOnlyList<Item> Closet,
    StyleContext Context,
    int Variation);
