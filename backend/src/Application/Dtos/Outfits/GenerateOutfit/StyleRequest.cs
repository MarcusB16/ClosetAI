namespace ClosetAPI.Application.Dtos.Outfits.GenerateOutfit;

public record StyleRequest
{
    public required string Prompt { get; init; }
    public string? LockItemId { get; init; }
    public bool? AvoidRecent { get; init; }
}
