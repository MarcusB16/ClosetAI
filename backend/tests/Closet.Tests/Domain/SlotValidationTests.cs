using Closet.Tests.Support;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;

namespace Closet.Tests.Domain;

public class SlotValidationTests
{
    private static readonly List<Item> Closet = TestClosets.Small();

    private static readonly OutfitPieces Pieces = new()
    {
        Outer = "o-smart", Top = "t-oxford", Bottom = "b-chino", Shoes = "s-loafer", Acc = ["a-watch", "a-belt"],
    };

    private static Item Item(string id) => Closet.Single(item => item.Id == id);

    [Theory]
    [InlineData(Slot.Shoes, "t-tee")]
    [InlineData(Slot.Top, "s-sneaker")]
    [InlineData(Slot.Outer, "b-jeans")]
    [InlineData(Slot.Bottom, "a-belt")]
    [InlineData(Slot.Acc, "t-oxford")]
    public void Replacing_with_an_item_from_another_slot_is_rejected(Slot slot, string itemId)
    {
        var exception = Assert.Throws<RuleViolationException>(() => PieceReplacer.ReplacePiece(Pieces, slot, Item(itemId), Closet));

        Assert.Equal("itemId", exception.Field);
    }

    [Fact]
    public void Replaces_single_slot_pieces()
    {
        Assert.Equal("s-sneaker", PieceReplacer.ReplacePiece(Pieces, Slot.Shoes, Item("s-sneaker"), Closet).Shoes);
        Assert.Equal("t-tee", PieceReplacer.ReplacePiece(Pieces, Slot.Top, Item("t-tee"), Closet).Top);
        Assert.Equal("b-jeans", PieceReplacer.ReplacePiece(Pieces, Slot.Bottom, Item("b-jeans"), Closet).Bottom);
        Assert.Equal("o-casual", PieceReplacer.ReplacePiece(Pieces, Slot.Outer, Item("o-casual"), Closet).Outer);
    }

    [Fact]
    public void Replacing_an_accessory_keeps_the_watch_and_swaps_the_other_one()
    {
        var replaced = PieceReplacer.ReplacePiece(Pieces, Slot.Acc, Item("a-beanie"), Closet);

        Assert.Equal(["a-watch", "a-beanie"], replaced.Acc);
    }

    [Fact]
    public void Replacing_the_watch_swaps_only_the_watch()
    {
        var other = ItemBuilder.Acc("a-watch2", "watch").Build();
        var closet = Closet.Append(other).ToList();

        var replaced = PieceReplacer.ReplacePiece(Pieces, Slot.Acc, other, closet);

        Assert.Equal(["a-watch2", "a-belt"], replaced.Acc);
    }

    [Fact]
    public void Adding_an_accessory_when_none_of_that_kind_exists_appends_it()
    {
        var pieces = Pieces with { Acc = ["a-watch"] };

        var replaced = PieceReplacer.ReplacePiece(pieces, Slot.Acc, Item("a-belt"), Closet);

        Assert.Equal(["a-watch", "a-belt"], replaced.Acc);
    }

    [Fact]
    public void Replacing_an_accessory_with_one_already_worn_does_not_duplicate_it()
    {
        var replaced = PieceReplacer.ReplacePiece(Pieces, Slot.Acc, Item("a-belt"), Closet);

        Assert.Equal(["a-watch", "a-belt"], replaced.Acc);
    }
}
