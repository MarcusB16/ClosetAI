using System.ComponentModel.DataAnnotations;

namespace ClosetAPI.Application.Dtos.Outfits.ReplacePiece;

public record ReplacePieceRequest
{
    [MinLength(1)]
    public required string ItemId { get; init; }
}
