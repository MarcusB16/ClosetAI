namespace ClosetAPI.Application.Dtos.Wears.LogWear;

public record WearEntryResponse
{
    public required DateOnly Date { get; init; }
    public required string OutfitId { get; init; }
}
