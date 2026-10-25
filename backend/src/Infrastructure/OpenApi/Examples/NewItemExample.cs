using ClosetAPI.Application.Dtos.Items.CreateItem;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class NewItemExample : IOpenApiExample
{
    public static object Value => new NewItem
    {
        Name = "White leather sneakers",
        Brand = "Common Projects",
        Category = Category.Shoes,
        Slot = Slot.Shoes,
        Type = "sneaker",
        Color = "#F2F1EC",
        ColorName = "White",
        Formality = 1,
        Fabric = "Italian calf leather",
        Size = "10",
        Seasons = ["Spring", "Summer", "Fall"],
    };
}
