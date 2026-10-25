namespace ClosetAPI.Domain.Outfits;

public interface IOutfitStylist
{
    Task<StyledOutfit> StyleAsync(StyleBrief brief, CancellationToken cancellationToken);
    Task<StyledOutfit> RefineAsync(RefineBrief brief, CancellationToken cancellationToken);
}
