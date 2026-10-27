namespace ClosetAPI.Domain.Items;

public record ItemTags(
    string Name,
    string? Brand,
    Category Category,
    Slot Slot,
    string Type,
    string ColorName,
    string Color,
    int Formality,
    IReadOnlyList<string> Seasons,
    string? Fabric);
