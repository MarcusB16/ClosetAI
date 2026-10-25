using Closet.Tests.Support;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Media;
using ClosetAPI.Domain.TryOns;

namespace Closet.Tests.Domain;

public class StubTests
{
    [Fact]
    public async Task Fake_tagger_returns_a_valid_suggestion()
    {
        var tags = await new FakeItemTagger().TagAsync(new PhotoContent(TestImages.Png, "image/png"), CancellationToken.None);

        Assert.True(GarmentTypes.IsKnown(tags.Type));
        Assert.True(Formality.IsValid(tags.Formality));
        Assert.Matches("^#[0-9A-F]{6}$", tags.Color);
    }

    [Fact]
    public async Task Background_remover_returns_the_original_photo()
    {
        Assert.Equal("photo.png", await new PassThroughBackgroundRemover().RemoveBackgroundAsync("photo.png", CancellationToken.None));
    }

    [Fact]
    public async Task Placeholder_renderer_returns_the_person_photo_flagged_as_placeholder()
    {
        var rendered = await new PlaceholderTryOnRenderer().RenderAsync("me.png", [], CancellationToken.None);

        Assert.Equal("me.png", rendered.ImageFile);
        Assert.True(rendered.Placeholder);
    }

    [Theory]
    [InlineData("tee", true)]
    [InlineData("tote", true)]
    [InlineData("spaceship", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Garment_types_are_validated(string? type, bool known)
    {
        Assert.Equal(known, GarmentTypes.IsKnown(type));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public void Formality_range_is_one_to_three(int value, bool valid)
    {
        Assert.Equal(valid, Formality.IsValid(value));
    }
}
