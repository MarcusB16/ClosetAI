namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class ItemListExample : IOpenApiExample
{
    public static object Value => new[] { ExampleData.Sneakers };
}
