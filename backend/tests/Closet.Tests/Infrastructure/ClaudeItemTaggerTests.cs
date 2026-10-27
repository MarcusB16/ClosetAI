using Closet.Tests.Support;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Media;
using ClosetAPI.Infrastructure.Ai;

namespace Closet.Tests.Infrastructure;

public class ClaudeItemTaggerTests
{
    private static readonly PhotoContent Photo = new(TestImages.Png, "image/png");

    private static string Json(string color = "#25314F", string type = "sweater", int formality = 2, string category = "shirts", string brand = "Everlane", string fabric = "wool") =>
        $$"""
        {"name":"Navy merino crewneck","brand":"{{brand}}","category":"{{category}}","slot":"top","type":"{{type}}","colorName":"Navy","color":"{{color}}","formality":{{formality}},"seasons":["Fall","Winter"],"fabric":"{{fabric}}"}
        """;

    [Fact]
    public async Task Maps_model_output_to_tags_and_sends_the_photo()
    {
        var llm = new ScriptedStructuredLlm(Json());

        var tags = await new ClaudeItemTagger(llm).TagPhotoAsync(Photo, CancellationToken.None);

        Assert.Equal("Navy merino crewneck", tags.Name);
        Assert.Equal(Category.Shirts, tags.Category);
        Assert.Equal(Slot.Top, tags.Slot);
        Assert.Equal("sweater", tags.Type);
        Assert.Equal(2, tags.Formality);
        Assert.Equal(["Fall", "Winter"], tags.Seasons);
        Assert.Same(Photo, llm.LastRequest!.Image);
    }

    [Fact]
    public async Task Empty_brand_and_fabric_become_null()
    {
        var tags = await new ClaudeItemTagger(new ScriptedStructuredLlm(Json(brand: "", fabric: " "))).TagPhotoAsync(Photo, CancellationToken.None);

        Assert.Null(tags.Brand);
        Assert.Null(tags.Fabric);
    }

    [Theory]
    [InlineData("navy", "sweater", 2, "shirts")]
    [InlineData("#25314F", "spaceship", 2, "shirts")]
    [InlineData("#25314F", "sweater", 7, "shirts")]
    [InlineData("#25314F", "sweater", 2, "nonsense")]
    public async Task Rejects_invalid_model_output(string color, string type, int formality, string category)
    {
        var tagger = new ClaudeItemTagger(new ScriptedStructuredLlm(Json(color, type, formality, category)));

        await Assert.ThrowsAsync<AiProviderException>(() => tagger.TagPhotoAsync(Photo, CancellationToken.None));
    }

    [Fact]
    public void Schema_lists_every_garment_type_and_category()
    {
        var properties = ItemSuggestionSchema.Build()["properties"]!;

        Assert.Equal(GarmentTypes.All.Count, properties["type"]!["enum"]!.AsArray().Count);
        Assert.Contains("formal", properties["category"]!["enum"]!.AsArray().Select(node => node!.GetValue<string>()));
    }
}
