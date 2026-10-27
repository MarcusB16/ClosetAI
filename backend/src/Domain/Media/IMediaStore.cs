namespace ClosetAPI.Domain.Media;

public interface IMediaStore
{
    Task<string> SavePhotoAsync(PhotoContent photo, CancellationToken cancellationToken);
    Task<PhotoContent?> ReadPhotoAsync(string fileName, CancellationToken cancellationToken);
}
