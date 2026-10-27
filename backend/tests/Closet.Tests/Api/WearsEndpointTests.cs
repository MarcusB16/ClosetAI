using System.Net;
using Closet.Tests.Support;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Application.Dtos.Wears.GetWornOutfits;
using ClosetAPI.Application.Dtos.Wears.LogWear;

namespace Closet.Tests.Api;

public class WearsEndpointTests(ApiFactory factory) : ApiTestBase(factory)
{
    private async Task<ItemResponse> Item(string id) => await Client.GetJsonAsync<ItemResponse>($"/api/items/{id}");

    [Fact]
    public async Task Lists_seeded_wear_history_newest_first_with_outfits()
    {
        var worn = await Client.GetJsonAsync<List<WornOutfit>>("/api/wears");

        Assert.Equal(7, worn.Count);
        Assert.Equal(["w1", "w2", "w3", "w4", "w5", "w6", "w7"], worn.Select(entry => entry.OutfitId));
        Assert.Equal(new DateOnly(2026, 10, 25), worn[0].Date);
        Assert.Equal("Studio day", worn[0].Outfit.Title);
        Assert.Equal(worn.Select(entry => entry.Date).OrderByDescending(date => date), worn.Select(entry => entry.Date));
    }

    [Fact]
    public async Task Logging_a_wear_increments_each_piece_and_sets_last_worn()
    {
        var before = await Item("s4");
        var watchBefore = await Item("a1");

        var response = await Client.PostAsync("/api/outfits/v1/wears", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var entry = await response.ReadAsync<WearEntryResponse>();
        Assert.Equal(new DateOnly(2026, 10, 26), entry.Date);
        Assert.Equal("v1", entry.OutfitId);
        var after = await Item("s4");
        Assert.Equal(before.TimesWorn + 1, after.TimesWorn);
        Assert.Equal(new DateOnly(2026, 10, 26), after.LastWorn);
        Assert.Equal(watchBefore.TimesWorn + 1, (await Item("a1")).TimesWorn);
        Assert.Equal("v1", (await Client.GetJsonAsync<List<WornOutfit>>("/api/wears"))[0].OutfitId);
    }

    [Fact]
    public async Task Logging_twice_on_the_same_day_replaces_the_first_entry()
    {
        var shoesBefore = await Item("s2");
        var chelseaBefore = await Item("s4");

        await Client.PostAsync("/api/outfits/v2/wears", null);
        await Client.PostAsync("/api/outfits/v1/wears", null);

        Assert.Equal(shoesBefore.TimesWorn, (await Item("s2")).TimesWorn);
        Assert.Equal(chelseaBefore.TimesWorn + 1, (await Item("s4")).TimesWorn);
        var today = (await Client.GetJsonAsync<List<WornOutfit>>("/api/wears")).Where(entry => entry.Date == new DateOnly(2026, 10, 26)).ToList();
        Assert.Single(today);
        Assert.Equal("v1", today[0].OutfitId);
    }

    [Fact]
    public async Task Logging_the_same_outfit_twice_does_not_double_count()
    {
        var before = await Item("s4");

        await Client.PostAsync("/api/outfits/v1/wears", null);
        await Client.PostAsync("/api/outfits/v1/wears", null);

        Assert.Equal(before.TimesWorn + 1, (await Item("s4")).TimesWorn);
    }
}
