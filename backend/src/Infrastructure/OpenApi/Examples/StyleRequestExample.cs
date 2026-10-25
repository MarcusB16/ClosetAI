using ClosetAPI.Application.Dtos.Outfits.GenerateOutfit;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class StyleRequestExample : IOpenApiExample
{
    public static object Value => new StyleRequest
    {
        Prompt = "Casual coffee with a friend",
        LockItemId = "s1",
        AvoidRecent = true,
    };
}
