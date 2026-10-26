using ClosetAPI.Domain.TryOns;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public class TryOnRenderRepository(ClosetDbContext db) : ITryOnRenderRepository
{
    public Task<TryOnRender?> FindAsync(string outfitId, string piecesKey, string personPhotoFile, CancellationToken cancellationToken) =>
        db.TryOnRenders.FirstOrDefaultAsync(
            render => render.OutfitId == outfitId && render.PiecesKey == piecesKey && render.PersonPhotoFile == personPhotoFile,
            cancellationToken);

    public void Add(TryOnRender render) => db.TryOnRenders.Add(render);
}
