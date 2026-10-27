using ClosetAPI.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public class UserStateRepository(ClosetDbContext db) : IUserStateRepository
{
    public async Task<UserState> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var state = await db.UserStates.FirstOrDefaultAsync(cancellationToken);
        if (state is not null) return state;

        state = new UserState();
        db.UserStates.Add(state);
        return state;
    }
}
