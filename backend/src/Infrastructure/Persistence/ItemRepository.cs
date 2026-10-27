using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public class ItemRepository(ClosetDbContext db) : IItemRepository
{
    public async Task<Item> GetAsync(string id, CancellationToken cancellationToken) =>
        await db.Items.FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw new EntityNotFoundException("Item", id);

    public async Task<List<Item>> ListNewestFirstAsync(CancellationToken cancellationToken)
    {
        var items = await db.Items.ToListAsync(cancellationToken);
        return items.OrderByDescending(item => item.AddedAt).ToList();
    }

    public void Add(Item item) => db.Items.Add(item);
}
