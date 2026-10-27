using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.TryOns;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace Closet.Tests.Support;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"closet-{Guid.NewGuid():N}";
    private SqliteConnection? _keepAlive;

    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));
    public string MediaFolder { get; } = Path.Combine(Path.GetTempPath(), $"closet-media-{Guid.NewGuid():N}");
    public string EnvironmentName { get; init; } = "Development";
    public IOutfitStylist? Stylist { get; init; }
    public IItemTagger? Tagger { get; init; }
    public ITryOnRenderer? Renderer { get; init; }
    public Dictionary<string, string?> Settings { get; init; } = [];

    private string ConnectionString => $"Data Source=file:{_databaseName}?mode=memory&cache=shared";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _keepAlive = new SqliteConnection(ConnectionString);
        _keepAlive.Open();

        builder.UseEnvironment(EnvironmentName);
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("ConnectionStrings:Closet", ConnectionString);
        builder.UseSetting("Media:Folder", MediaFolder);
        foreach (var (key, value) in Settings) builder.UseSetting(key, value);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
            Replace(services, Stylist);
            Replace(services, Tagger);
            Replace(services, Renderer);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        _keepAlive?.Dispose();
        if (Directory.Exists(MediaFolder)) Directory.Delete(MediaFolder, recursive: true);
    }

    private static void Replace<TService>(IServiceCollection services, TService? replacement) where TService : class
    {
        if (replacement is null) return;
        services.RemoveAll<TService>();
        services.AddSingleton(replacement);
    }
}
