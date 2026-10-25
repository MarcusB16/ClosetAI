using ClosetAPI.Domain.Items;

namespace Closet.Tests.Support;

public class ItemBuilder
{
    private readonly Item _item;

    private ItemBuilder(string id, Slot slot, string type)
    {
        _item = new Item
        {
            Id = id,
            Name = $"Item {id}",
            Brand = "Brand",
            Category = CategoryFor(slot),
            Slot = slot,
            Type = type,
            Color = "#101010",
            ColorName = "Black",
            Formality = Formality.Smart,
            Fabric = "cotton",
            Size = "M",
            AddedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
    }

    public static ItemBuilder Top(string id, string type = "shirt") => new(id, Slot.Top, type);
    public static ItemBuilder Bottom(string id, string type = "pants") => new(id, Slot.Bottom, type);
    public static ItemBuilder Shoes(string id, string type = "sneaker") => new(id, Slot.Shoes, type);
    public static ItemBuilder Outer(string id, string type = "jacket") => new(id, Slot.Outer, type);
    public static ItemBuilder Acc(string id, string type = "belt") => new(id, Slot.Acc, type);

    public ItemBuilder Named(string name) { _item.Name = name; return this; }
    public ItemBuilder WithFormality(int formality) { _item.Formality = formality; return this; }
    public ItemBuilder WithCategory(Category category) { _item.Category = category; return this; }
    public ItemBuilder WithKeywords(params string[] keywords) { _item.Keywords = keywords.ToList(); return this; }
    public ItemBuilder WornOn(DateOnly date) { _item.LastWorn = date; return this; }
    public ItemBuilder NoRain() { _item.NoRain = true; return this; }
    public ItemBuilder RainOk() { _item.RainOk = true; return this; }

    public Item Build() => _item;

    public static implicit operator Item(ItemBuilder builder) => builder.Build();

    private static Category CategoryFor(Slot slot) => slot switch
    {
        Slot.Top => Category.Shirts,
        Slot.Bottom => Category.Pants,
        Slot.Outer => Category.Jackets,
        Slot.Shoes => Category.Shoes,
        _ => Category.Accessories,
    };
}
