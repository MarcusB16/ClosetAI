namespace ClosetAPI.Domain.Items;

public class PassThroughBackgroundRemover : IBackgroundRemover
{
    public Task<string> RemoveBackgroundAsync(string photoFileName, CancellationToken cancellationToken) =>
        Task.FromResult(photoFileName);
}
