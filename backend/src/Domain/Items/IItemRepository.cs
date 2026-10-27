namespace ClosetAPI.Domain.Items;

public interface IItemRepository
{
    Task<Item> GetAsync(string id, CancellationToken cancellationToken);
    Task<List<Item>> ListNewestFirstAsync(CancellationToken cancellationToken);
    void Add(Item item);
}
