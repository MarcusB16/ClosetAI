namespace ClosetAPI.Domain.Wears;

public interface IWearRepository
{
    Task<WearEntry?> FindByDateAsync(DateOnly date, CancellationToken cancellationToken);
    Task<List<WearEntry>> ListNewestFirstAsync(CancellationToken cancellationToken);
    void Add(WearEntry wear);
}
