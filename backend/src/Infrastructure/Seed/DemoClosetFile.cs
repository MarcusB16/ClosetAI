namespace ClosetAPI.Infrastructure.Seed;

public record DemoClosetFile
{
    public required List<SeedItem> Items { get; init; }
    public required List<SeedOutfit> Outfits { get; init; }
    public required List<string> TodayIds { get; init; }
}
