using System.ComponentModel.DataAnnotations;

namespace ClosetAPI.Application.Dtos.Outfits.RefineOutfit;

public record RefineRequest
{
    [MinLength(1)]
    public required string Instruction { get; init; }
}
