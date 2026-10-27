using ClosetAPI.Application.Dtos.Wears.GetWornOutfits;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class WornOutfitListExample : IOpenApiExample
{
    public static object Value => new[]
    {
        new WornOutfit
        {
            Date = new DateOnly(2026, 10, 25),
            OutfitId = ExampleData.Outfit.Id,
            Outfit = ExampleData.Outfit,
        },
    };
}
