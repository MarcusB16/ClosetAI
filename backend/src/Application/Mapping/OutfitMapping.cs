using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Domain.Outfits;

namespace ClosetAPI.Application.Mapping;

public static class OutfitMapping
{
    public static OutfitResponse ToResponse(this Outfit outfit) => new()
    {
        Id = outfit.Id,
        Title = outfit.Title,
        Prompt = outfit.Prompt,
        Note = outfit.Note,
        Saved = outfit.Saved,
        CreatedAt = outfit.CreatedAt,
        Pieces = new OutfitPiecesResponse
        {
            Outer = outfit.Pieces.Outer,
            Top = outfit.Pieces.Top,
            Bottom = outfit.Pieces.Bottom,
            Shoes = outfit.Pieces.Shoes,
            Acc = outfit.Pieces.Acc.ToList(),
        },
    };
}
