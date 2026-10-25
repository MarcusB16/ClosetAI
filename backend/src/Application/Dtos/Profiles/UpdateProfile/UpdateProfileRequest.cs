namespace ClosetAPI.Application.Dtos.Profiles.UpdateProfile;

public record UpdateProfileRequest
{
    public bool? Onboarded { get; init; }
}
