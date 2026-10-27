using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.Persistence;
using ClosetAPI.Domain.TryOns;
using ClosetAPI.Domain.Users;
using ClosetAPI.Domain.Wears;
using ClosetAPI.Infrastructure.Persistence;
using ClosetAPI.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Extensions;

public static class PersistenceServiceExtensions
{
    public const string ProviderKey = "Database:Provider";
    public const string MigrateOnStartupKey = "Database:MigrateOnStartup";
    public const string ConnectionStringName = "Closet";

    public static IServiceCollection AddClosetPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");
        var provider = configuration[ProviderKey] ?? "Sqlite";

        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            services.AddDbContext<ClosetDbContext, SqlServerClosetDbContext>(options => options.UseSqlServer(connectionString));
        else if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            services.AddDbContext<ClosetDbContext, SqliteClosetDbContext>(options => options.UseSqlite(connectionString));
        else
            throw new InvalidOperationException($"Unsupported {ProviderKey} '{provider}'. Use 'SqlServer' or 'Sqlite'.");

        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<IOutfitRepository, OutfitRepository>();
        services.AddScoped<IWearRepository, WearRepository>();
        services.AddScoped<ITryOnRenderRepository, TryOnRenderRepository>();
        services.AddScoped<IUserStateRepository, UserStateRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DemoDataSeeder>();
        return services;
    }

    public static async Task InitializeClosetDatabaseAsync(this WebApplication app)
    {
        var shouldMigrate = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>(MigrateOnStartupKey);
        if (!shouldMigrate) return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClosetDbContext>();
        await db.Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedIfEmptyAsync(CancellationToken.None);
    }
}
