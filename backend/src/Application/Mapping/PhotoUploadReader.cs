using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Media;

namespace ClosetAPI.Application.Mapping;

public static class PhotoUploadReader
{
    public static async Task<PhotoContent> ReadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length > PhotoValidator.MaxBytes)
            throw new RuleViolationException("photo", "The photo is larger than 10 MB.");

        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();

        var problem = PhotoValidator.FindValidationError(bytes);
        if (problem is not null) throw new RuleViolationException("photo", problem);

        return new PhotoContent(bytes, PhotoValidator.DetectContentType(bytes)!);
    }
}
