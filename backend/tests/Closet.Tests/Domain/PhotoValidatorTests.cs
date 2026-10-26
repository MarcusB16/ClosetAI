using Closet.Tests.Support;
using ClosetAPI.Domain.Media;

namespace Closet.Tests.Domain;

public class PhotoValidatorTests
{
    [Fact]
    public void Accepts_png_and_jpeg()
    {
        Assert.Null(PhotoValidator.FindValidationError(TestImages.Png));
        Assert.Null(PhotoValidator.FindValidationError(TestImages.Jpeg));
        Assert.Equal("image/png", PhotoValidator.DetectContentType(TestImages.Png));
        Assert.Equal("image/jpeg", PhotoValidator.DetectContentType(TestImages.Jpeg));
    }

    [Fact]
    public void Rejects_empty_oversized_and_unknown_files()
    {
        Assert.NotNull(PhotoValidator.FindValidationError([]));
        Assert.Contains("10 MB", PhotoValidator.FindValidationError(TestImages.Oversized()));
        Assert.Contains("JPEG or PNG", PhotoValidator.FindValidationError("hello"u8.ToArray()));
        Assert.Null(PhotoValidator.DetectContentType("hello"u8.ToArray()));
    }

    [Fact]
    public void Picks_extension_from_content_type()
    {
        Assert.Equal(".png", PhotoValidator.ExtensionFor("image/png"));
        Assert.Equal(".jpg", PhotoValidator.ExtensionFor("image/jpeg"));
    }
}
