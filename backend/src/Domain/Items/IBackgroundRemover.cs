namespace ClosetAPI.Domain.Items;

public interface IBackgroundRemover
{
    Task<string> RemoveBackgroundAsync(string photoFileName, CancellationToken cancellationToken);
}
