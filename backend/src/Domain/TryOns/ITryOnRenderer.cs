using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.TryOns;

public interface ITryOnRenderer
{
    Task<RenderedTryOn> RenderAsync(string personPhotoFile, IReadOnlyList<Item> pieces, CancellationToken cancellationToken);
}
