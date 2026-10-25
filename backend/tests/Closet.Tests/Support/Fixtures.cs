using ClosetAPI.Application.Dtos.Items.CreateItem;
using ClosetAPI.Domain.Items;

namespace Closet.Tests.Support;

public static class Fixtures
{
    public static NewItem NewItem(string name = "Test tee", Slot slot = Slot.Top, string type = "tee", Category category = Category.Shirts, int formality = 1) => new()
    {
        Name = name,
        Brand = "Brand",
        Category = category,
        Slot = slot,
        Type = type,
        Color = "#112233",
        ColorName = "Navy",
        Formality = formality,
        Fabric = "cotton",
        Size = "M",
    };
}
