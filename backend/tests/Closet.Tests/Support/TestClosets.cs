using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Items;

namespace Closet.Tests.Support;

public static class TestClosets
{
    public static readonly DateOnly Today = new(2026, 10, 26);

    public static StyleContext Dry => new(Today, "Monday", 70, "Sunny", Rain: false);
    public static StyleContext Rainy => new(Today, "Monday", 55, "Rain after 4pm", Rain: true);

    public static List<Item> Small() =>
    [
        ItemBuilder.Outer("o-casual", "jacket").Named("Denim jacket").WithFormality(1).Build(),
        ItemBuilder.Outer("o-smart", "blazer").Named("Navy blazer").WithFormality(2).Build(),
        ItemBuilder.Outer("o-rain", "jacket").Named("Waxed jacket").WithFormality(2).RainOk().Build(),
        ItemBuilder.Top("t-tee", "tee").Named("Black tee").WithFormality(1).Build(),
        ItemBuilder.Top("t-oxford", "shirt").Named("White oxford").WithFormality(2).Build(),
        ItemBuilder.Bottom("b-jeans", "jeans").Named("Indigo jeans").WithFormality(1).Build(),
        ItemBuilder.Bottom("b-chino", "pants").Named("Stone chinos").WithFormality(2).Build(),
        ItemBuilder.Bottom("b-shorts", "shorts").Named("Navy shorts").WithFormality(1).NoRain().Build(),
        ItemBuilder.Shoes("s-sneaker", "sneaker").Named("White sneakers").WithFormality(1).Build(),
        ItemBuilder.Shoes("s-loafer", "loafer").Named("Brown loafers").WithFormality(2).Build(),
        ItemBuilder.Acc("a-watch", "watch").Named("Field watch").Build(),
        ItemBuilder.Acc("a-belt", "belt").Named("Brown belt").Build(),
        ItemBuilder.Acc("a-beanie", "beanie").Named("Navy beanie").WithFormality(1).Build(),
    ];

    public static List<Item> Formal() =>
    [
        ItemBuilder.Outer("f-jacket", "blazer").Named("Charcoal suit jacket").WithFormality(3).WithCategory(Category.Formal).WithKeywords("suit").Build(),
        ItemBuilder.Top("f-shirt", "shirt").Named("White dress shirt").WithFormality(3).WithCategory(Category.Formal).Build(),
        ItemBuilder.Bottom("f-trousers", "pants").Named("Charcoal suit trousers").WithFormality(3).WithCategory(Category.Formal).Build(),
        ItemBuilder.Shoes("f-shoes", "derby").Named("Black oxfords").WithFormality(3).WithCategory(Category.Formal).Build(),
        ItemBuilder.Acc("f-tie", "tie").Named("Navy tie").WithFormality(3).WithCategory(Category.Formal).Build(),
        ItemBuilder.Top("t-tee", "tee").Named("Black tee").WithFormality(1).Build(),
        ItemBuilder.Bottom("b-jeans", "jeans").Named("Indigo jeans").WithFormality(1).Build(),
        ItemBuilder.Shoes("s-sneaker", "sneaker").Named("White sneakers").WithFormality(1).Build(),
    ];
}
