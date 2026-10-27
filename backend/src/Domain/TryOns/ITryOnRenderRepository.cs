namespace ClosetAPI.Domain.TryOns;

public interface ITryOnRenderRepository
{
    Task<TryOnRender?> FindAsync(string outfitId, string piecesKey, string personPhotoFile, CancellationToken cancellationToken);
    void Add(TryOnRender render);
}
