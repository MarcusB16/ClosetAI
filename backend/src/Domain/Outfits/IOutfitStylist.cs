namespace ClosetAPI.Domain.Outfits;

public interface IOutfitStylist
{
    Task<StyledOutfit> StyleOutfitAsync(StyleBrief brief, CancellationToken cancellationToken);
    Task<StyledOutfit> RefineOutfitAsync(RefineBrief brief, CancellationToken cancellationToken);
}
