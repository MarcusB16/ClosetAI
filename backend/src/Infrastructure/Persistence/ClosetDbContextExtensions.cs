using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public static class ClosetDbContextExtensions
{
    public static async Task<Outfit> GetOutfitAsync(this ClosetDbContext db, string id, CancellationToken cancellationToken) =>
        await db.Outfits.FirstOrDefaultAsync(outfit => outfit.Id == id, cancellationToken)
        ?? throw new EntityNotFoundException("Outfit", id);

    public static async Task<Item> GetItemAsync(this ClosetDbContext db, string id, CancellationToken cancellationToken) =>
        await db.Items.FirstOrDefaultAsync(item => item.Id == id, cancellationToken)
        ?? throw new EntityNotFoundException("Item", id);

    public static async Task<List<Item>> LoadClosetAsync(this ClosetDbContext db, CancellationToken cancellationToken)
    {
        var items = await db.Items.ToListAsync(cancellationToken);
        return items.OrderByDescending(item => item.AddedAt).ToList();
    }

    public static async Task<UserState> GetUserStateAsync(this ClosetDbContext db, CancellationToken cancellationToken)
    {
        var state = await db.UserStates.FirstOrDefaultAsync(cancellationToken);
        if (state is not null) return state;

        state = new UserState();
        db.UserStates.Add(state);
        return state;
    }

    public static string NewId() => Guid.NewGuid().ToString("N");
}
