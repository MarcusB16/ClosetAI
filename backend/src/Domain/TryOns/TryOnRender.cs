namespace ClosetAPI.Domain.TryOns;

public class TryOnRender
{
    public string Id { get; set; } = "";
    public string OutfitId { get; set; } = "";
    public string PiecesKey { get; set; } = "";
    public string PersonPhotoFile { get; set; } = "";
    public string ImageFile { get; set; } = "";
    public bool Placeholder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
