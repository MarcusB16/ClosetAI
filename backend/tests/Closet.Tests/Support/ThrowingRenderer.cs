using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.TryOns;

namespace Closet.Tests.Support;

public class ThrowingRenderer : ITryOnRenderer
{
    public Task<RenderedTryOn> RenderTryOnAsync(string personPhotoFile, IReadOnlyList<Item> pieces, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("image model down");
}
