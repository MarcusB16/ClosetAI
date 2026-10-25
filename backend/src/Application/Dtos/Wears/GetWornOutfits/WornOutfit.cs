using ClosetAPI.Domain.Outfits;

namespace ClosetAPI.Application.Dtos.Wears.GetWornOutfits;

public record WornOutfit
{
    public required DateOnly Date { get; init; }
    public required string OutfitId { get; init; }
    public required Outfit Outfit { get; init; }
}
