namespace ClosetAPI.Domain.Media;

public static class PhotoValidator
{
    public const long MaxBytes = 10 * 1024 * 1024;

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    public static string? Validate(byte[] bytes)
    {
        if (bytes.Length == 0) return "The photo is empty.";
        if (bytes.Length > MaxBytes) return "The photo is larger than 10 MB.";
        return DetectContentType(bytes) is null ? "The photo must be a JPEG or PNG image." : null;
    }

    public static string? DetectContentType(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith(PngSignature)) return "image/png";
        if (bytes.AsSpan().StartsWith(JpegSignature)) return "image/jpeg";
        return null;
    }

    public static string ExtensionFor(string contentType) => contentType == "image/png" ? ".png" : ".jpg";
}
