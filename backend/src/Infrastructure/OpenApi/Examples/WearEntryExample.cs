using ClosetAPI.Application.Dtos.Wears.LogWear;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class WearEntryExample : IOpenApiExample
{
    public static object Value => new WearEntryResponse
    {
        Date = new DateOnly(2026, 10, 26),
        OutfitId = ExampleData.Outfit.Id,
    };
}
