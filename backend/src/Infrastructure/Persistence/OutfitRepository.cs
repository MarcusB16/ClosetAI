using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Outfits;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public class OutfitRepository(ClosetDbContext db) : IOutfitRepository
{
    public async Task<Outfit> GetAsync(string id, CancellationToken cancellationToken) =>
        await FindAsync(id, cancellationToken) ?? throw new EntityNotFoundException("Outfit", id);

    public Task<Outfit?> FindAsync(string id, CancellationToken cancellationToken) =>
        db.Outfits.FirstOrDefaultAsync(outfit => outfit.Id == id, cancellationToken);

    public Task<List<Outfit>> ListByIdsAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken) =>
        db.Outfits.Where(outfit => ids.Contains(outfit.Id)).ToListAsync(cancellationToken);

    public async Task<List<Outfit>> ListSavedNewestFirstAsync(CancellationToken cancellationToken)
    {
        var saved = await db.Outfits.Where(outfit => outfit.Saved).ToListAsync(cancellationToken);
        return saved.OrderByDescending(outfit => outfit.CreatedAt).ToList();
    }

    public void Add(Outfit outfit) => db.Outfits.Add(outfit);
}
