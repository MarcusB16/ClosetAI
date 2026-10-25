using Closet.Tests.Support;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Outfits;

namespace Closet.Tests.Domain;

public class OutfitModelTests
{
    private readonly List<ClosetAPI.Domain.Items.Item> _closet = TestClosets.Small();

    [Fact]
    public void PiecesBuilder_assigns_ids_to_their_slots()
    {
        var pieces = PiecesBuilder.FromIds(["a-belt", "s-loafer", "o-smart", "t-tee", "b-jeans", "a-watch"], _closet);

        Assert.Equal("o-smart", pieces.Outer);
        Assert.Equal("t-tee", pieces.Top);
        Assert.Equal("b-jeans", pieces.Bottom);
        Assert.Equal("s-loafer", pieces.Shoes);
        Assert.Equal(["a-belt", "a-watch"], pieces.Acc);
    }

    [Fact]
    public void PiecesBuilder_rejects_unknown_ids()
    {
        Assert.Throws<EntityNotFoundException>(() => PiecesBuilder.FromIds(["ghost"], _closet));
    }

    [Fact]
    public void AllIds_skips_missing_pieces()
    {
        var pieces = new OutfitPieces { Top = "t", Bottom = "b", Shoes = "s", Acc = ["a"] };

        Assert.Equal(["t", "b", "s", "a"], pieces.AllIds());
    }

    [Fact]
    public void Key_ignores_accessory_order()
    {
        var first = new OutfitPieces { Top = "t", Bottom = "b", Shoes = "s", Acc = ["a1", "a2"] };
        var second = first with { Acc = ["a2", "a1"] };

        Assert.Equal(first.Key(), second.Key());
        Assert.NotEqual(first.Key(), (first with { Top = "t2" }).Key());
    }

    [Fact]
    public void Average_formality_uses_the_four_main_slots()
    {
        var pieces = new OutfitPieces { Outer = "o-smart", Top = "t-tee", Bottom = "b-jeans", Shoes = "s-loafer" };

        Assert.Equal((2 + 1 + 1 + 2) / 4.0, OutfitFormality.Average(pieces, _closet));
    }

    [Fact]
    public void Average_formality_defaults_to_smart_for_unknown_items()
    {
        Assert.Equal(2, OutfitFormality.Average(new OutfitPieces { Top = "x", Bottom = "y", Shoes = "z" }, _closet));
        Assert.Equal(2, OutfitFormality.Average(new OutfitPieces { Top = "", Bottom = "", Shoes = "" }, _closet));
    }

    [Fact]
    public void SeededRandom_is_deterministic_and_in_range()
    {
        var first = new SeededRandom(42);
        var second = new SeededRandom(42);

        for (var i = 0; i < 20; i++)
        {
            var value = first.NextDouble();
            Assert.Equal(value, second.NextDouble());
            Assert.InRange(value, 0, 0.9999999);
        }
        Assert.InRange(new SeededRandom(1).NextIndex(5), 0, 4);
    }
}
