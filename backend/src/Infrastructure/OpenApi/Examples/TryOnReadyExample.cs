using ClosetAPI.Application.Dtos.TryOns.GetTryOn;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class TryOnReadyExample : IOpenApiExample
{
    public static object Value => new TryOnResult
    {
        Status = TryOnStatus.Ready,
        ImageUri = "http://localhost:5080/media/9a8b7c6d5e4f30211a2b3c4d5e6f7a8b.jpg",
        Placeholder = true,
    };
}
