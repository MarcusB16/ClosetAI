using ClosetAPI.Domain.Wears;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public class WearRepository(ClosetDbContext db) : IWearRepository
{
    public Task<WearEntry?> FindByDateAsync(DateOnly date, CancellationToken cancellationToken) =>
        db.Wears.FirstOrDefaultAsync(wear => wear.Date == date, cancellationToken);

    public async Task<List<WearEntry>> ListNewestFirstAsync(CancellationToken cancellationToken)
    {
        var wears = await db.Wears.ToListAsync(cancellationToken);
        return wears.OrderByDescending(wear => wear.Date).ToList();
    }

    public void Add(WearEntry wear) => db.Wears.Add(wear);
}
