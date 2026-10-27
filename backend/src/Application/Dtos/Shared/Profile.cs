namespace ClosetAPI.Application.Dtos.Shared;

public record Profile
{
    public required bool Onboarded { get; init; }
    public string? PersonPhotoUri { get; init; }
}
