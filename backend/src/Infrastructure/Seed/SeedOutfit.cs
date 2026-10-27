namespace ClosetAPI.Infrastructure.Seed;

public record SeedOutfit
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required List<string> Ids { get; init; }
    public int? WornDaysAgo { get; init; }
    public bool Saved { get; init; }
}
