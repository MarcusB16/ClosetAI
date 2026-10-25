using ClosetAPI.Domain.Media;

namespace ClosetAPI.Domain.Items;

public class FakeItemTagger : IItemTagger
{
    public Task<ItemTags> TagAsync(PhotoContent photo, CancellationToken cancellationToken)
    {
        var suggestion = new ItemTags(
            Name: "New item",
            Brand: null,
            Category: Category.Shirts,
            Slot: Slot.Top,
            Type: "tee",
            ColorName: "Grey",
            Color: "#8A8A84",
            Formality: Formality.Casual,
            Seasons: ["Fall", "Winter"],
            Fabric: null);
        return Task.FromResult(suggestion);
    }
}
