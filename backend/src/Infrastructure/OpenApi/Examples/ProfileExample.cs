using ClosetAPI.Application.Dtos.Shared;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class ProfileExample : IOpenApiExample
{
    public static object Value => new Profile
    {
        Onboarded = true,
        PersonPhotoUri = "http://localhost:5080/media/9a8b7c6d5e4f30211a2b3c4d5e6f7a8b.jpg",
    };
}
