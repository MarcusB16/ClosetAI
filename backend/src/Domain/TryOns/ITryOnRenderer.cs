using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.TryOns;

public interface ITryOnRenderer
{
    Task<RenderedTryOn> RenderTryOnAsync(string personPhotoFile, IReadOnlyList<Item> pieces, CancellationToken cancellationToken);
}
