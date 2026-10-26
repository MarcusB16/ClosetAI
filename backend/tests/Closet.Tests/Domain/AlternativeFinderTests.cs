using Closet.Tests.Support;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;

namespace Closet.Tests.Domain;

public class AlternativeFinderTests
{
    private static readonly List<Item> Closet = TestClosets.Small();

    private static readonly OutfitPieces Pieces = new()
    {
        Outer = "o-smart", Top = "t-oxford", Bottom = "b-chino", Shoes = "s-loafer", Acc = ["a-watch", "a-belt"],
    };

    private static List<string> Ids(Slot slot, ClosetAPI.Domain.Context.StyleContext context) =>
        AlternativeFinder.FindAlternatives(Pieces, slot, Closet, context).Select(item => item.Id).ToList();

    [Fact]
    public void Excludes_the_current_piece()
    {
        Assert.DoesNotContain("s-loafer", Ids(Slot.Shoes, TestClosets.Dry));
        Assert.Contains("s-sneaker", Ids(Slot.Shoes, TestClosets.Dry));
    }

    [Fact]
    public void Only_returns_items_for_the_requested_slot()
    {
        Assert.All(AlternativeFinder.FindAlternatives(Pieces, Slot.Bottom, Closet, TestClosets.Dry), item => Assert.Equal(Slot.Bottom, item.Slot));
    }

    [Fact]
    public void Excludes_items_that_must_stay_dry_when_it_rains()
    {
        Assert.Contains("b-shorts", Ids(Slot.Bottom, TestClosets.Dry).Concat(new[] { "b-shorts" }));
        Assert.DoesNotContain("b-shorts", Ids(Slot.Bottom, TestClosets.Rainy));
    }

    [Fact]
    public void Excludes_items_too_far_from_the_outfit_formality()
    {
        var formal = ItemBuilder.Shoes("s-formal", "derby").WithFormality(3).Build();
        var closet = Closet.Append(formal).ToList();
        var casualPieces = Pieces with { Top = "t-tee", Bottom = "b-jeans", Shoes = "s-sneaker", Outer = "o-casual" };

        var ids = AlternativeFinder.FindAlternatives(casualPieces, Slot.Shoes, closet, TestClosets.Dry).Select(item => item.Id);

        Assert.DoesNotContain("s-formal", ids);
    }

    [Fact]
    public void Accessory_alternatives_skip_watches_and_the_current_non_watch_accessory()
    {
        var ids = Ids(Slot.Acc, TestClosets.Dry);

        Assert.DoesNotContain("a-watch", ids);
        Assert.DoesNotContain("a-belt", ids);
        Assert.Contains("a-beanie", ids);
    }
}
