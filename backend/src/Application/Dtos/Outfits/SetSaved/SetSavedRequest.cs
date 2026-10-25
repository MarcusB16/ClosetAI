namespace ClosetAPI.Application.Dtos.Outfits.SetSaved;

public record SetSavedRequest
{
    public required bool Saved { get; init; }
}
