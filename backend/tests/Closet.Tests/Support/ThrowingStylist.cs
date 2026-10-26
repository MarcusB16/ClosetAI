using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Outfits;

namespace Closet.Tests.Support;

public class ThrowingStylist : IOutfitStylist
{
    public Task<StyledOutfit> StyleOutfitAsync(StyleBrief brief, CancellationToken cancellationToken) =>
        throw new AiProviderException("boom");

    public Task<StyledOutfit> RefineOutfitAsync(RefineBrief brief, CancellationToken cancellationToken) =>
        throw new AiProviderException("boom");
}
