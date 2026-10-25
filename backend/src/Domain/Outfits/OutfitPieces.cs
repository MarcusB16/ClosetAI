namespace ClosetAPI.Domain.Outfits;

public record OutfitPieces
{
    public string? Outer { get; init; }
    public string Top { get; init; } = "";
    public string Bottom { get; init; } = "";
    public string Shoes { get; init; } = "";
    public List<string> Acc { get; init; } = [];

    public IEnumerable<string> AllIds() =>
        new[] { Outer, Top, Bottom, Shoes }
            .Concat(Acc)
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => id!);

    public string Key() => string.Join('|', new[] { Outer ?? "", Top, Bottom, Shoes }.Concat(Acc.Order()));
}
