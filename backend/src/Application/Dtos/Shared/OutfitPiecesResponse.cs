namespace ClosetAPI.Application.Dtos.Shared;

public record OutfitPiecesResponse
{
    public required string Top { get; init; }
    public required string Bottom { get; init; }
    public required string Shoes { get; init; }
    public required List<string> Acc { get; init; }
    public string? Outer { get; init; }
}
