namespace ClosetAPI.Domain.TryOns;

public class TryOnRender
{
    public string OutfitId { get; set; } = "";
    public string PiecesKey { get; set; } = "";
    public string PersonPhotoUri { get; set; } = "";
    public string ImageUri { get; set; } = "";
    public bool Placeholder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
