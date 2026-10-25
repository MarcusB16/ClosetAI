namespace ClosetAPI.Domain.Outfits;

public record OutfitPieces(string? Outer, string Top, string Bottom, string Shoes, List<string> Acc)
{
    public IEnumerable<string> AllIds() =>
        new[] { Outer, Top, Bottom, Shoes }.Concat(Acc).Where(id => !string.IsNullOrEmpty(id))!;
}
