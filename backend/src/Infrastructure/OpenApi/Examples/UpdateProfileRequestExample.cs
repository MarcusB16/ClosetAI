using ClosetAPI.Application.Dtos.Profiles.UpdateProfile;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class UpdateProfileRequestExample : IOpenApiExample
{
    public static object Value => new UpdateProfileRequest { Onboarded = true };
}
