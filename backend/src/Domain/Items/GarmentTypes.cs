namespace ClosetAPI.Domain.Items;

public static class GarmentTypes
{
    public const string Watch = "watch";

    private static readonly HashSet<string> Known = new(StringComparer.Ordinal)
    {
        "tee", "shirt", "sweater", "blazer", "coat", "jacket",
        "pants", "jeans", "shorts",
        "sneaker", "derby", "loafer", "boot",
        "watch", "belt", "sunglasses", "scarf", "beanie", "tie", "tote",
    };

    public static IReadOnlyCollection<string> All => Known;

    public static bool IsKnown(string? type) => type is not null && Known.Contains(type);
}
