namespace ClosetAPI.Application.Dtos.Profiles.GetProfile;

public record Profile
{
    public required bool Onboarded { get; init; }
    public required string? PersonPhotoUri { get; init; }
}
