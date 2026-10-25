namespace ClosetAPI.Domain.Media;

public interface IMediaStore
{
    Task<string> SaveAsync(PhotoContent photo, CancellationToken cancellationToken);
    Task<PhotoContent?> ReadAsync(string fileName, CancellationToken cancellationToken);
}
