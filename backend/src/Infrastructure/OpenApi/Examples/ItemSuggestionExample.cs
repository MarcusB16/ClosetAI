using ClosetAPI.Application.Dtos.Items.SuggestItem;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class ItemSuggestionExample : IOpenApiExample
{
    public static object Value => new ItemSuggestion
    {
        Name = "Navy merino crewneck",
        Brand = "Everlane",
        Category = Category.Shirts,
        Slot = Slot.Top,
        Type = "sweater",
        ColorName = "Navy",
        Color = "#25314F",
        Formality = 2,
        Seasons = ["Fall", "Winter"],
        Fabric = "100% merino wool",
        CutoutUri = "http://localhost:5080/media/3c9f0d2a1b7e4f6a8d5c0e1f2a3b4c5d.jpg",
    };
}
