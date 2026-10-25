using ClosetAPI.Domain.Media;
using Microsoft.Extensions.Options;

namespace ClosetAPI.Infrastructure.Media;

public class LocalMediaStore : IMediaStore
{
    private readonly string _folder;

    public LocalMediaStore(IOptions<MediaOptions> options, IHostEnvironment environment)
    {
        _folder = Path.GetFullPath(options.Value.Folder, environment.ContentRootPath);
        Directory.CreateDirectory(_folder);
    }

    public string Folder => _folder;

    public async Task<string> SaveAsync(PhotoContent photo, CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid():N}{PhotoValidator.ExtensionFor(photo.ContentType)}";
        await File.WriteAllBytesAsync(Path.Combine(_folder, fileName), photo.Bytes, cancellationToken);
        return fileName;
    }

    public async Task<PhotoContent?> ReadAsync(string fileName, CancellationToken cancellationToken)
    {
        if (fileName != Path.GetFileName(fileName)) return null;

        var path = Path.Combine(_folder, fileName);
        if (!File.Exists(path)) return null;

        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        var contentType = PhotoValidator.DetectContentType(bytes);
        return contentType is null ? null : new PhotoContent(bytes, contentType);
    }
}
