using Closet.Tests.Api;
using Closet.Tests.Support;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.Persistence;
using ClosetAPI.Domain.TryOns;
using ClosetAPI.Domain.Users;
using ClosetAPI.Domain.Wears;
using ClosetAPI.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Closet.Tests.Infrastructure;

public class RepositoryTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static readonly DateOnly SeededWearDate = new(2026, 10, 25);

    private async Task<TResult> InScope<TService, TResult>(Func<TService, Task<TResult>> use) where TService : notnull
    {
        using var scope = Factory.Services.CreateScope();
        return await use(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private async Task Save(Action<IServiceProvider> change)
    {
        using var scope = Factory.Services.CreateScope();
        change(scope.ServiceProvider);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Getting_an_unknown_item_throws_not_found()
    {
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            InScope<IItemRepository, Item>(items => items.GetAsync("missing", CancellationToken.None)));
    }

    [Fact]
    public async Task Items_are_listed_newest_first()
    {
        Item newest = ItemBuilder.Top("newest").Build();
        newest.AddedAt = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await Save(services => services.GetRequiredService<IItemRepository>().Add(newest));

        var closet = await InScope<IItemRepository, List<Item>>(items => items.ListNewestFirstAsync(CancellationToken.None));

        Assert.Equal("newest", closet[0].Id);
        Assert.Equal(closet.Select(item => item.AddedAt).OrderByDescending(addedAt => addedAt), closet.Select(item => item.AddedAt));
    }

    [Fact]
    public async Task Added_items_are_only_persisted_when_the_unit_of_work_saves()
    {
        using (var scope = Factory.Services.CreateScope())
            scope.ServiceProvider.GetRequiredService<IItemRepository>().Add(ItemBuilder.Top("unsaved"));
        await Save(services => services.GetRequiredService<IItemRepository>().Add(ItemBuilder.Top("saved")));

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            InScope<IItemRepository, Item>(items => items.GetAsync("unsaved", CancellationToken.None)));
        var saved = await InScope<IItemRepository, Item>(items => items.GetAsync("saved", CancellationToken.None));
        Assert.Equal("saved", saved.Id);
    }

    [Fact]
    public async Task Finding_an_unknown_outfit_returns_null_and_getting_it_throws()
    {
        Assert.Null(await InScope<IOutfitRepository, Outfit?>(outfits => outfits.FindAsync("missing", CancellationToken.None)));
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            InScope<IOutfitRepository, Outfit>(outfits => outfits.GetAsync("missing", CancellationToken.None)));
    }

    [Fact]
    public async Task Outfits_listed_by_id_include_only_the_requested_ones_that_exist()
    {
        var found = await InScope<IOutfitRepository, List<Outfit>>(outfits =>
            outfits.ListByIdsAsync(["w1", "w3", "missing"], CancellationToken.None));

        Assert.Equal(["w1", "w3"], found.Select(outfit => outfit.Id).Order());
    }

    [Fact]
    public async Task Saved_outfits_are_listed_newest_first()
    {
        var saved = await InScope<IOutfitRepository, List<Outfit>>(outfits => outfits.ListSavedNewestFirstAsync(CancellationToken.None));

        Assert.Equal(4, saved.Count);
        Assert.All(saved, outfit => Assert.True(outfit.Saved));
        Assert.Equal(saved.Select(outfit => outfit.CreatedAt).OrderByDescending(createdAt => createdAt), saved.Select(outfit => outfit.CreatedAt));
    }

    [Fact]
    public async Task Wears_are_found_by_date()
    {
        var seeded = await InScope<IWearRepository, WearEntry?>(wears => wears.FindByDateAsync(SeededWearDate, CancellationToken.None));
        var unworn = await InScope<IWearRepository, WearEntry?>(wears => wears.FindByDateAsync(new DateOnly(2020, 1, 1), CancellationToken.None));

        Assert.Equal("w1", seeded?.OutfitId);
        Assert.Null(unworn);
    }

    [Fact]
    public async Task Wears_are_listed_newest_first()
    {
        var history = await InScope<IWearRepository, List<WearEntry>>(wears => wears.ListNewestFirstAsync(CancellationToken.None));

        Assert.Equal(SeededWearDate, history[0].Date);
        Assert.Equal(history.Select(wear => wear.Date).OrderByDescending(date => date), history.Select(wear => wear.Date));
    }

    [Fact]
    public async Task A_cached_try_on_render_matches_only_on_outfit_pieces_and_photo()
    {
        await Save(services => services.GetRequiredService<ITryOnRenderRepository>().Add(new TryOnRender
        {
            Id = EntityId.New(),
            OutfitId = "w1",
            PiecesKey = "pieces",
            PersonPhotoFile = "me.jpg",
            ImageFile = "render.png",
            CreatedAt = Factory.Clock.GetUtcNow(),
        }));

        Task<TryOnRender?> Find(string outfitId, string piecesKey, string photo) =>
            InScope<ITryOnRenderRepository, TryOnRender?>(renders => renders.FindAsync(outfitId, piecesKey, photo, CancellationToken.None));

        Assert.Equal("render.png", (await Find("w1", "pieces", "me.jpg"))?.ImageFile);
        Assert.Null(await Find("w2", "pieces", "me.jpg"));
        Assert.Null(await Find("w1", "other-pieces", "me.jpg"));
        Assert.Null(await Find("w1", "pieces", "other.jpg"));
    }

    [Fact]
    public async Task The_seeded_user_state_is_returned()
    {
        var state = await InScope<IUserStateRepository, UserState>(userStates => userStates.GetOrCreateAsync(CancellationToken.None));

        Assert.Equal(UserState.SingleUserId, state.Id);
        Assert.NotNull(state.TodayOutfitId);
    }

    [Fact]
    public async Task A_missing_user_state_is_created_and_persisted_on_save()
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ClosetDbContext>();
            db.UserStates.RemoveRange(db.UserStates);
            await db.SaveChangesAsync();
        }

        using (var scope = Factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IUserStateRepository>().GetOrCreateAsync(CancellationToken.None);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
        }
        var state = await InScope<IUserStateRepository, UserState>(userStates => userStates.GetOrCreateAsync(CancellationToken.None));

        Assert.Equal(UserState.SingleUserId, state.Id);
        Assert.False(state.Onboarded);
        Assert.Null(state.TodayOutfitId);
    }
}
