using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.TryOns;

namespace Closet.Tests.Support;

public class CountingRenderer : ITryOnRenderer
{
    public int Calls { get; private set; }

    public Task<RenderedTryOn> RenderAsync(string personPhotoFile, IReadOnlyList<Item> pieces, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(new RenderedTryOn(personPhotoFile, Placeholder: false));
    }
}
