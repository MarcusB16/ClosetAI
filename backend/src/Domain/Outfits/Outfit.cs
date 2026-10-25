namespace ClosetAPI.Domain.Outfits;

public class Outfit
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Prompt { get; set; }
    public string? Note { get; set; }
    public OutfitPieces Pieces { get; set; } = new();
    public bool Saved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
