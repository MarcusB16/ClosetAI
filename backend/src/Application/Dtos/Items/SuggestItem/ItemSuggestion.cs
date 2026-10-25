using ClosetAPI.Domain.Items;

namespace ClosetAPI.Application.Dtos.Items.SuggestItem;

public record ItemSuggestion
{
    public required string Name { get; init; }
    public required string? Brand { get; init; }
    public required Category Category { get; init; }
    public required string ColorName { get; init; }
    public required string Color { get; init; }
    public required double Formality { get; init; }
    public required List<string> Seasons { get; init; }
    public required string? Fabric { get; init; }
    public required string CutoutUri { get; init; }
    public Slot? Slot { get; init; }
    public string? Type { get; init; }
}
