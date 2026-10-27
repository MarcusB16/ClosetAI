using Closet.Tests.Support;
using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;

namespace Closet.Tests.Domain;

public class RulesOutfitStylistTests
{
    private readonly RulesOutfitStylist _stylist = new();

    private Task<StyledOutfit> Style(string prompt, List<Item> closet, StyleContext? context = null, string? lockId = null, bool avoidRecent = false, int variation = 4) =>
        _stylist.StyleOutfitAsync(new StyleBrief(prompt, lockId, avoidRecent, closet, context ?? TestClosets.Dry, variation), CancellationToken.None);

    private static Item Find(List<Item> closet, string id) => closet.Single(item => item.Id == id);

    [Fact]
    public async Task Formal_prompt_builds_the_formal_outfit()
    {
        var closet = TestClosets.Formal();

        var outfit = await Style("I have a wedding to attend", closet);

        Assert.Equal("f-shirt", outfit.Pieces.Top);
        Assert.Equal("f-trousers", outfit.Pieces.Bottom);
        Assert.Equal("f-shoes", outfit.Pieces.Shoes);
        Assert.Equal("f-jacket", outfit.Pieces.Outer);
        Assert.Contains("f-tie", outfit.Pieces.Acc);
        Assert.Equal("Formal", outfit.Title);
    }

    [Fact]
    public async Task Casual_prompt_never_uses_formal_category_items()
    {
        var closet = TestClosets.Formal();

        var outfit = await Style("a relaxed weekend walk", closet);

        Assert.Equal("t-tee", outfit.Pieces.Top);
        Assert.Equal("b-jeans", outfit.Pieces.Bottom);
        Assert.Equal("s-sneaker", outfit.Pieces.Shoes);
        Assert.Equal("Easy casual", outfit.Title);
    }

    [Fact]
    public async Task Locked_item_is_included_and_named_in_the_title()
    {
        var closet = TestClosets.Small();

        var outfit = await Style("something nice", closet, lockId: "s-loafer");

        Assert.Equal("s-loafer", outfit.Pieces.Shoes);
        Assert.Equal("Around the loafers", outfit.Title);
        Assert.Contains("Built around your brown loafers.", outfit.Note);
    }

    [Fact]
    public async Task Locked_accessory_is_added_after_the_watch()
    {
        var closet = TestClosets.Small();

        var outfit = await Style("anything", closet, lockId: "a-beanie");

        Assert.Equal(["a-watch", "a-beanie"], outfit.Pieces.Acc);
    }

    [Fact]
    public async Task Item_mentioned_by_name_or_keyword_is_locked()
    {
        var closet = TestClosets.Formal();

        var outfit = await Style("wear my suit", closet);

        Assert.Equal("f-jacket", outfit.Pieces.Outer);
        Assert.StartsWith("Around the", outfit.Title);
    }

    [Fact]
    public void FindMentionedItem_returns_null_when_nothing_matches()
    {
        Assert.Null(RulesOutfitStylist.FindMentionedItem("nothing relevant", TestClosets.Small()));
    }

    [Fact]
    public async Task Rain_excludes_items_that_must_stay_dry()
    {
        var closet = TestClosets.Small();

        for (var variation = 1; variation <= 20; variation++)
        {
            var outfit = await Style("casual day", closet, TestClosets.Rainy, variation: variation);
            Assert.NotEqual("b-shorts", outfit.Pieces.Bottom);
        }
    }

    [Fact]
    public async Task Rain_mentions_forecast_when_the_outer_layer_is_rain_ready()
    {
        var closet = new List<Item>(TestClosets.Small().Where(item => item.Slot != Slot.Outer || item.Id == "o-rain"));

        var outfit = await Style("anything", closet, TestClosets.Rainy);

        Assert.Equal("o-rain", outfit.Pieces.Outer);
        Assert.Contains("Rain after 4pm, so the waxed jacket goes on top.", outfit.Note);
        Assert.Equal("Monday, rain later", outfit.Title);
    }

    [Fact]
    public async Task Dry_weather_note_mentions_the_temperature()
    {
        var closet = TestClosets.Small();

        var outfit = await Style("anything", closet, TestClosets.Dry);

        Assert.Contains("It's 70° today", outfit.Note);
        Assert.Equal("Monday", outfit.Title);
    }

    [Fact]
    public async Task Avoid_recent_skips_items_worn_in_the_last_week_but_keeps_the_watch()
    {
        var closet = TestClosets.Small();
        var recent = TestClosets.Today.AddDays(-2);
        foreach (var item in closet.Where(item => item.Id is "t-tee" or "b-jeans" or "s-sneaker" or "a-watch"))
            item.LastWorn = recent;

        for (var variation = 1; variation <= 10; variation++)
        {
            var outfit = await Style("casual", closet, avoidRecent: true, variation: variation);
            Assert.NotEqual("t-tee", outfit.Pieces.Top);
            Assert.NotEqual("b-jeans", outfit.Pieces.Bottom);
            Assert.NotEqual("s-sneaker", outfit.Pieces.Shoes);
            Assert.Contains("a-watch", outfit.Pieces.Acc);
            Assert.Equal("Nothing here was worn in the last week.", outfit.Note.Split(". ")[0] + ".");
        }
    }

    [Fact]
    public async Task Prompt_wording_can_ask_to_avoid_recent_wears()
    {
        var closet = TestClosets.Small();
        var outfit = await Style("show me something different", closet);

        Assert.Equal("Something new", outfit.Title);
    }

    [Fact]
    public async Task Dinner_pulls_the_target_down_and_casual_dinner_is_named()
    {
        var closet = TestClosets.Small();

        Assert.Equal("Dinner", (await Style("dinner with clients at the office", closet)).Title);
        Assert.Equal("Casual dinner", (await Style("casual dinner", closet)).Title);
    }

    [Fact]
    public async Task Business_prompt_picks_smart_pieces()
    {
        var closet = TestClosets.Small();

        var outfit = await Style("a client meeting", closet);

        Assert.Equal("Business casual", outfit.Title);
        Assert.Equal("t-oxford", outfit.Pieces.Top);
        Assert.Equal("b-chino", outfit.Pieces.Bottom);
        Assert.Equal("s-loafer", outfit.Pieces.Shoes);
    }

    [Fact]
    public async Task Same_variation_gives_the_same_outfit()
    {
        var closet = TestClosets.Small();

        var first = await Style("anything", closet, variation: 9);
        var second = await Style("anything", closet, variation: 9);

        Assert.Equal(first.Pieces.AllItemIds().ToList(), second.Pieces.AllItemIds().ToList());
    }

    [Fact]
    public async Task Different_variations_eventually_give_different_outfits()
    {
        var closet = TestClosets.Small();

        var outfits = new HashSet<string>();
        for (var variation = 1; variation <= 30; variation++)
            outfits.Add((await Style("anything", closet, variation: variation)).Pieces.ToCacheKey());

        Assert.True(outfits.Count > 1);
    }

    [Fact]
    public async Task Outer_layer_is_optional()
    {
        var closet = TestClosets.Small().Where(item => item.Slot != Slot.Outer).ToList();

        var outfit = await Style("anything", closet);

        Assert.Null(outfit.Pieces.Outer);
        Assert.DoesNotContain("goes on top", outfit.Note);
    }

    [Theory]
    [InlineData(Slot.Top)]
    [InlineData(Slot.Bottom)]
    [InlineData(Slot.Shoes)]
    public async Task Missing_required_slot_is_a_rule_violation(Slot missing)
    {
        var closet = TestClosets.Small().Where(item => item.Slot != missing).ToList();

        var exception = await Assert.ThrowsAsync<RuleViolationException>(() => Style("anything", closet));

        Assert.Contains(missing.ToString(), exception.Message);
    }

    [Fact]
    public async Task Refine_with_a_slot_instruction_swaps_only_that_slot()
    {
        var closet = TestClosets.Small();
        var current = new Outfit
        {
            Title = "Keep me",
            Note = "Original note",
            Pieces = new OutfitPieces { Outer = "o-smart", Top = "t-oxford", Bottom = "b-chino", Shoes = "s-loafer", Acc = ["a-watch"] },
        };

        var refined = await _stylist.RefineOutfitAsync(new RefineBrief(current, "different shoes", closet, TestClosets.Dry, 5), CancellationToken.None);

        Assert.Equal("s-sneaker", refined.Pieces.Shoes);
        Assert.Equal(current.Pieces.Top, refined.Pieces.Top);
        Assert.Equal(current.Pieces.Bottom, refined.Pieces.Bottom);
        Assert.Equal(current.Pieces.Outer, refined.Pieces.Outer);
        Assert.Equal("Keep me", refined.Title);
        Assert.Equal("Original note", refined.Note);
    }

    [Fact]
    public async Task Refine_with_no_alternatives_returns_the_same_pieces()
    {
        var closet = TestClosets.Small().Where(item => item.Id != "s-sneaker").ToList();
        var current = new Outfit
        {
            Title = "Same",
            Pieces = new OutfitPieces { Outer = "o-smart", Top = "t-oxford", Bottom = "b-chino", Shoes = "s-loafer", Acc = ["a-watch"] },
        };

        var refined = await _stylist.RefineOutfitAsync(new RefineBrief(current, "other footwear", closet, TestClosets.Dry, 5), CancellationToken.None);

        Assert.Equal(current.Pieces.ToCacheKey(), refined.Pieces.ToCacheKey());
        Assert.Equal("Same", refined.Title);
    }

    [Fact]
    public async Task Refine_without_a_slot_restyles_using_prompt_and_instruction()
    {
        var closet = TestClosets.Formal();
        var current = new Outfit { Title = "x", Prompt = "a day out", Pieces = new OutfitPieces { Top = "t-tee", Bottom = "b-jeans", Shoes = "s-sneaker" } };

        var refined = await _stylist.RefineOutfitAsync(new RefineBrief(current, "make it a wedding", closet, TestClosets.Dry, 5), CancellationToken.None);

        Assert.Equal("Formal", refined.Title);
    }

    [Fact]
    public async Task Refine_without_a_prompt_on_the_original_still_works()
    {
        var closet = TestClosets.Small();
        var current = new Outfit { Title = "x", Pieces = new OutfitPieces { Top = "t-tee", Bottom = "b-jeans", Shoes = "s-sneaker" } };

        var refined = await _stylist.RefineOutfitAsync(new RefineBrief(current, "more casual please", closet, TestClosets.Dry, 5), CancellationToken.None);

        Assert.Equal("Easy casual", refined.Title);
    }
}
