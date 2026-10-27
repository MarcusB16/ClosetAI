using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.TryOns;
using ClosetAPI.Domain.Users;
using ClosetAPI.Domain.Wears;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Persistence;

public abstract class ClosetDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Outfit> Outfits => Set<Outfit>();
    public DbSet<WearEntry> Wears => Set<WearEntry>();
    public DbSet<TryOnRender> TryOnRenders => Set<TryOnRender>();
    public DbSet<UserState> UserStates => Set<UserState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(item =>
        {
            item.HasKey(i => i.Id);
            item.Property(i => i.Category).HasConversion<string>().HasMaxLength(32);
            item.Property(i => i.Slot).HasConversion<string>().HasMaxLength(32);
            item.Property(i => i.Price).HasPrecision(10, 2);
            item.Property(i => i.Variant).HasJsonConversion();
            item.Property(i => i.Keywords).HasJsonConversion();
            item.Property(i => i.Seasons).HasJsonConversion();
            item.Ignore(i => i.IsWatch);
        });

        modelBuilder.Entity<Outfit>(outfit =>
        {
            outfit.HasKey(o => o.Id);
            outfit.ComplexProperty(o => o.Pieces, pieces => pieces.Property(p => p.Acc).HasJsonConversion());
        });

        modelBuilder.Entity<WearEntry>().HasKey(w => w.Date);
        modelBuilder.Entity<TryOnRender>().HasKey(t => t.Id);
        modelBuilder.Entity<UserState>(user =>
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.Id).ValueGeneratedNever();
        });
    }
}
