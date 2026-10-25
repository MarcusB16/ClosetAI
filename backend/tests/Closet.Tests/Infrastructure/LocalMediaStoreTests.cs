using Closet.Tests.Support;
using ClosetAPI.Domain.Media;
using ClosetAPI.Infrastructure.Media;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Closet.Tests.Infrastructure;

public class LocalMediaStoreTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"closet-store-{Guid.NewGuid():N}");
    private readonly LocalMediaStore _store;

    public LocalMediaStoreTests()
    {
        _store = new LocalMediaStore(Options.Create(new MediaOptions { Folder = _folder }), new FakeEnvironment());
    }

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Fact]
    public async Task Saves_and_reads_back_a_photo()
    {
        var name = await _store.SaveAsync(new PhotoContent(TestImages.Png, "image/png"), CancellationToken.None);

        var photo = await _store.ReadAsync(name, CancellationToken.None);

        Assert.EndsWith(".png", name);
        Assert.Equal(TestImages.Png, photo!.Bytes);
        Assert.Equal("image/png", photo.ContentType);
    }

    [Fact]
    public async Task Missing_files_and_path_traversal_return_null()
    {
        Assert.Null(await _store.ReadAsync("missing.png", CancellationToken.None));
        Assert.Null(await _store.ReadAsync("../secret.png", CancellationToken.None));
    }

    [Fact]
    public async Task Non_image_files_are_not_served_back()
    {
        await File.WriteAllTextAsync(Path.Combine(_folder, "note.txt"), "hello");

        Assert.Null(await _store.ReadAsync("note.txt", CancellationToken.None));
    }

    [Fact]
    public void Url_resolver_falls_back_to_a_relative_path_outside_a_request()
    {
        var resolver = new MediaUrlResolver(new Microsoft.AspNetCore.Http.HttpContextAccessor());

        Assert.Equal("/media/a.png", resolver.UrlFor("a.png"));
        Assert.Null(resolver.UrlForOrNull(null));
        Assert.Null(resolver.UrlForOrNull(""));
    }

    private class FakeEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
