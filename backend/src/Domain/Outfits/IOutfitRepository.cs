namespace ClosetAPI.Domain.Outfits;

public interface IOutfitRepository
{
    Task<Outfit> GetAsync(string id, CancellationToken cancellationToken);
    Task<Outfit?> FindAsync(string id, CancellationToken cancellationToken);
    Task<List<Outfit>> ListByIdsAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken);
    Task<List<Outfit>> ListSavedNewestFirstAsync(CancellationToken cancellationToken);
    void Add(Outfit outfit);
}
