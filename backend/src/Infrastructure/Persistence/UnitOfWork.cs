using ClosetAPI.Domain.Persistence;

namespace ClosetAPI.Infrastructure.Persistence;

public class UnitOfWork(ClosetDbContext db) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
