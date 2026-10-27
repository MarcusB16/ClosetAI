namespace ClosetAPI.Application.Dtos.Shared;

public record OutfitResponse
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required OutfitPiecesResponse Pieces { get; init; }
    public required bool Saved { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public string? Prompt { get; init; }
    public string? Note { get; init; }
}
