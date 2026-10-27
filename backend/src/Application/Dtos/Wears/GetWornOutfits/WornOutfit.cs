using ClosetAPI.Application.Dtos.Shared;

namespace ClosetAPI.Application.Dtos.Wears.GetWornOutfits;

public record WornOutfit
{
    public required DateOnly Date { get; init; }
    public required string OutfitId { get; init; }
    public required OutfitResponse Outfit { get; init; }
}
