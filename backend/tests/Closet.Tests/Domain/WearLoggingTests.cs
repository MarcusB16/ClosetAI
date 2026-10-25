using Closet.Tests.Support;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.Wears;

namespace Closet.Tests.Domain;

public class WearLoggingTests
{
    private readonly List<Item> _closet = TestClosets.Small();

    private Outfit Outfit(params string[] ids) => new() { Id = "o", Pieces = PiecesBuilder.FromIds(ids, _closet) };

    private Item Item(string id) => _closet.Single(item => item.Id == id);

    [Fact]
    public void Wearing_an_outfit_increments_every_piece_and_sets_last_worn()
    {
        var outfit = Outfit("t-tee", "b-jeans", "s-sneaker", "a-watch");

        WearRecorder.Apply(TestClosets.Today, outfit, replacedOutfit: null, _closet);

        foreach (var id in new[] { "t-tee", "b-jeans", "s-sneaker", "a-watch" })
        {
            Assert.Equal(1, Item(id).TimesWorn);
            Assert.Equal(TestClosets.Today, Item(id).LastWorn);
        }
        Assert.Equal(0, Item("t-oxford").TimesWorn);
        Assert.Null(Item("t-oxford").LastWorn);
    }

    [Fact]
    public void Replacing_the_days_outfit_undoes_the_earlier_increments()
    {
        var first = Outfit("t-tee", "b-jeans", "s-sneaker");
        var second = Outfit("t-oxford", "b-jeans", "s-loafer");
        WearRecorder.Apply(TestClosets.Today, first, null, _closet);

        WearRecorder.Apply(TestClosets.Today, second, first, _closet);

        Assert.Equal(0, Item("t-tee").TimesWorn);
        Assert.Equal(0, Item("s-sneaker").TimesWorn);
        Assert.Equal(1, Item("b-jeans").TimesWorn);
        Assert.Equal(1, Item("t-oxford").TimesWorn);
    }

    [Fact]
    public void Undoing_never_drops_below_zero()
    {
        var replaced = Outfit("t-tee", "b-jeans", "s-sneaker");

        WearRecorder.Apply(TestClosets.Today, Outfit("t-oxford", "b-chino", "s-loafer"), replaced, _closet);

        Assert.Equal(0, Item("t-tee").TimesWorn);
    }

    [Fact]
    public void Pieces_missing_from_the_closet_are_ignored()
    {
        var outfit = new Outfit { Pieces = new OutfitPieces { Top = "ghost", Bottom = "b-jeans", Shoes = "s-sneaker" } };

        WearRecorder.Apply(TestClosets.Today, outfit, null, _closet);

        Assert.Equal(1, Item("b-jeans").TimesWorn);
    }

    [Fact]
    public void Marking_last_worn_only_moves_the_date_forward()
    {
        Item("t-tee").LastWorn = new DateOnly(2026, 10, 20);
        Item("b-jeans").LastWorn = new DateOnly(2026, 10, 25);

        WearRecorder.MarkLastWorn(new DateOnly(2026, 10, 22), Outfit("t-tee", "b-jeans", "s-sneaker"), _closet);

        Assert.Equal(new DateOnly(2026, 10, 22), Item("t-tee").LastWorn);
        Assert.Equal(new DateOnly(2026, 10, 25), Item("b-jeans").LastWorn);
        Assert.Equal(new DateOnly(2026, 10, 22), Item("s-sneaker").LastWorn);
        Assert.Equal(0, Item("t-tee").TimesWorn);
    }
}
