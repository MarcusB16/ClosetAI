namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class OutfitListExample : IOpenApiExample
{
    public static object Value => new[] { ExampleData.Outfit with { Saved = true } };
}
