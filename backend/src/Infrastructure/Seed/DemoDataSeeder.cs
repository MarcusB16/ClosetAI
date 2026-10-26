using System.Text.Json;
using System.Text.Json.Serialization;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.Persistence;
using ClosetAPI.Domain.Users;
using ClosetAPI.Domain.Wears;
using ClosetAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Infrastructure.Seed;

public class DemoDataSeeder(ClosetDbContext db, TimeProvider clock)
{
    private const string TodayTitleSuffix = ", rain later";
    private const string TodayNote = "Rain is due after 4pm, so the olive field jacket goes on top. Chelsea boots handle the puddles.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public async Task SeedIfEmptyAsync(CancellationToken cancellationToken)
    {
        if (await db.UserStates.AnyAsync(cancellationToken)) return;
        await SeedAsync(cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await db.TryOnRenders.ExecuteDeleteAsync(cancellationToken);
        await db.Wears.ExecuteDeleteAsync(cancellationToken);
        await db.Outfits.ExecuteDeleteAsync(cancellationToken);
        await db.Items.ExecuteDeleteAsync(cancellationToken);
        await db.UserStates.ExecuteDeleteAsync(cancellationToken);
        db.ChangeTracker.Clear();
        await SeedAsync(cancellationToken);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        var file = await ReadFileAsync(cancellationToken);
        var now = clock.GetLocalNow();
        var today = DateOnly.FromDateTime(now.DateTime);

        var items = file.Items.Select(seed => ToItem(seed, today, now)).ToList();
        var outfits = file.Outfits.Select((seed, index) => ToOutfit(seed, index, file.Outfits.Count, items, now)).ToList();
        var wears = ToWears(file.Outfits, today);
        foreach (var wear in wears)
            WearRecorder.MarkLastWorn(wear.Date, outfits.First(outfit => outfit.Id == wear.OutfitId), items);

        var todaysOutfit = BuildTodaysOutfit(file.TodayIds, items, today, now);
        outfits.Add(todaysOutfit);

        db.Items.AddRange(items);
        db.Outfits.AddRange(outfits);
        db.Wears.AddRange(wears);
        db.UserStates.Add(new UserState { TodayOutfitDate = today, TodayOutfitId = todaysOutfit.Id });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<DemoClosetFile> ReadFileAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Seed", "demo-closet.json");
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<DemoClosetFile>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("demo-closet.json is empty.");
    }

    private static Item ToItem(SeedItem seed, DateOnly today, DateTimeOffset now) => new()
    {
        Id = seed.Id,
        Name = seed.Name,
        Brand = seed.Brand,
        Category = seed.Category,
        Slot = seed.Slot,
        Type = seed.Type,
        Variant = seed.Variant,
        Color = seed.Color,
        ColorName = seed.ColorName,
        Formality = seed.Formality,
        Fabric = seed.Fabric,
        Size = seed.Size,
        Price = seed.Price,
        TimesWorn = seed.TimesWorn,
        LastWorn = today.AddDays(-seed.LastDaysAgo),
        Keywords = seed.Keywords,
        RainOk = seed.RainOk,
        NoRain = seed.NoRain,
        Seasons = seed.Seasons,
        AddedAt = now,
    };

    private static Outfit ToOutfit(SeedOutfit seed, int index, int count, List<Item> items, DateTimeOffset now) => new()
    {
        Id = seed.Id,
        Title = seed.Title,
        Pieces = PiecesBuilder.BuildFromItemIds(seed.Ids, items),
        Saved = seed.Saved,
        CreatedAt = now.AddSeconds(index - count),
    };

    private static List<WearEntry> ToWears(List<SeedOutfit> outfits, DateOnly today) =>
        outfits
            .Where(outfit => outfit.WornDaysAgo is > 0)
            .Select(outfit => new WearEntry { Date = today.AddDays(-outfit.WornDaysAgo!.Value), OutfitId = outfit.Id })
            .ToList();

    private static Outfit BuildTodaysOutfit(List<string> ids, List<Item> items, DateOnly today, DateTimeOffset now) => new()
    {
        Id = EntityId.New(),
        Title = $"{today.DayOfWeek}{TodayTitleSuffix}",
        Note = TodayNote,
        Pieces = PiecesBuilder.BuildFromItemIds(ids, items),
        CreatedAt = now,
    };
}
