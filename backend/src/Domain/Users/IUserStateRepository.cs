namespace ClosetAPI.Domain.Users;

public interface IUserStateRepository
{
    Task<UserState> GetOrCreateAsync(CancellationToken cancellationToken);
}
