using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.TryOns;

public class PlaceholderTryOnRenderer : ITryOnRenderer
{
    public Task<RenderedTryOn> RenderTryOnAsync(string personPhotoFile, IReadOnlyList<Item> pieces, CancellationToken cancellationToken) =>
        Task.FromResult(new RenderedTryOn(personPhotoFile, Placeholder: true));
}
