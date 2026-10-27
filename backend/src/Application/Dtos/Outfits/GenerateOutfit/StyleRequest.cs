using System.ComponentModel.DataAnnotations;

namespace ClosetAPI.Application.Dtos.Outfits.GenerateOutfit;

public record StyleRequest
{
    [MinLength(1)]
    public required string Prompt { get; init; }
    public string? LockItemId { get; init; }
    public bool? AvoidRecent { get; init; }
}
