using System.Text.Json;
using Closet.Tests.Support;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Infrastructure.Ai;

namespace Closet.Tests.Infrastructure;

public class ClaudeOutfitStylistTests
{
    private const string ValidJson = """
        {"title":"Smart casual","note":"Nice.","outer":"o-smart","top":"t-oxford","bottom":"b-chino","shoes":"s-loafer","acc":["a-watch","none"]}
        """;

    private static StyleBrief Brief(string? lockId = null) =>
        new("dinner", lockId, false, TestClosets.Small(), TestClosets.Dry, 1);

    [Fact]
    public async Task Builds_an_outfit_from_valid_model_output()
    {
        var llm = new ScriptedStructuredLlm(ValidJson);

        var outfit = await new ClaudeOutfitStylist(llm).StyleOutfitAsync(Brief(), CancellationToken.None);

        Assert.Equal("Smart casual", outfit.Title);
        Assert.Equal("o-smart", outfit.Pieces.Outer);
        Assert.Equal(["a-watch"], outfit.Pieces.Acc);
    }

    [Fact]
    public async Task Constrains_the_schema_to_ids_in_the_closet()
    {
        var llm = new ScriptedStructuredLlm(ValidJson);

        await new ClaudeOutfitStylist(llm).StyleOutfitAsync(Brief("s-loafer"), CancellationToken.None);

        var schema = llm.LastRequest!.Schema.ToJsonString();
        using var document = JsonDocument.Parse(schema);
        var shoes = document.RootElement.GetProperty("properties").GetProperty("shoes").GetProperty("enum")
            .EnumerateArray().Select(value => value.GetString()).ToList();
        Assert.Equal(["s-sneaker", "s-loafer"], shoes);
        Assert.Contains("\"lockedItemId\":\"s-loafer\"", llm.LastRequest.UserText);
    }

    [Theory]
    [InlineData("""{"title":"t","note":"n","outer":"none","top":"ghost","bottom":"b-chino","shoes":"s-loafer","acc":[]}""")]
    [InlineData("""{"title":"t","note":"n","outer":"none","top":"b-jeans","bottom":"b-chino","shoes":"s-loafer","acc":[]}""")]
    [InlineData("""{"title":"t","note":"n","outer":"t-tee","top":"t-oxford","bottom":"b-chino","shoes":"s-loafer","acc":[]}""")]
    [InlineData("""{"title":"t","note":"n","outer":"none","top":"t-oxford","bottom":"b-chino","shoes":"s-loafer","acc":["t-tee"]}""")]
    [InlineData("""{"note":"n","outer":"none","top":"t-oxford","bottom":"b-chino","shoes":"s-loafer","acc":[]}""")]
    [InlineData("""{"title":"t","note":"n","outer":"none","top":"t-oxford","bottom":"b-chino","shoes":"s-loafer"}""")]
    public async Task Rejects_model_output_that_breaks_the_closet_rules(string json)
    {
        var stylist = new ClaudeOutfitStylist(new ScriptedStructuredLlm(json));

        await Assert.ThrowsAsync<AiProviderException>(() => stylist.StyleOutfitAsync(Brief(), CancellationToken.None));
    }

    [Fact]
    public async Task Rejects_output_that_ignores_the_locked_item()
    {
        var stylist = new ClaudeOutfitStylist(new ScriptedStructuredLlm(ValidJson));

        await Assert.ThrowsAsync<AiProviderException>(() => stylist.StyleOutfitAsync(Brief("s-sneaker"), CancellationToken.None));
    }

    [Fact]
    public async Task Refine_sends_the_current_outfit_and_instruction()
    {
        var llm = new ScriptedStructuredLlm(ValidJson);
        var current = new Outfit { Title = "Old", Pieces = new OutfitPieces { Top = "t-tee", Bottom = "b-jeans", Shoes = "s-sneaker" } };

        var outfit = await new ClaudeOutfitStylist(llm).RefineOutfitAsync(
            new RefineBrief(current, "dress it up", TestClosets.Small(), TestClosets.Dry, 1), CancellationToken.None);

        Assert.Equal("Smart casual", outfit.Title);
        Assert.Contains("dress it up", llm.LastRequest!.UserText);
        Assert.Contains("\"top\":\"t-tee\"", llm.LastRequest.UserText);
    }

    [Fact]
    public async Task Refuses_to_style_a_closet_without_required_slots()
    {
        var closet = TestClosets.Small().Where(item => item.Slot != ClosetAPI.Domain.Items.Slot.Shoes).ToList();
        var stylist = new ClaudeOutfitStylist(new ScriptedStructuredLlm(ValidJson));

        await Assert.ThrowsAsync<RuleViolationException>(() =>
            stylist.StyleOutfitAsync(new StyleBrief("x", null, false, closet, TestClosets.Dry, 1), CancellationToken.None));
    }

    [Fact]
    public void Schema_offers_only_none_when_the_closet_has_no_accessories()
    {
        var closet = TestClosets.Small().Where(item => item.Slot != ClosetAPI.Domain.Items.Slot.Acc).ToList();

        var accItems = OutfitSchema.ForCloset(closet)["properties"]!["acc"]!["items"]!["enum"]!.AsArray();

        Assert.Equal(["none"], accItems.Select(node => node!.GetValue<string>()));
    }
}
