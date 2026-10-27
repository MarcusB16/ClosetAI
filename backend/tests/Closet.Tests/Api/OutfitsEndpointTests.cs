using System.Net;
using System.Net.Http.Json;
using Closet.Tests.Support;
using ClosetAPI.Application.Dtos.Outfits.GenerateOutfit;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Domain.Items;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Closet.Tests.Api;

public class OutfitsEndpointTests(ApiFactory factory) : ApiTestBase(factory)
{
    private Task<List<ItemResponse>> Closet() => Client.GetJsonAsync<List<ItemResponse>>("/api/items");

    [Fact]
    public async Task Saved_lists_the_four_seeded_saved_outfits_newest_first()
    {
        var saved = await Client.GetJsonAsync<List<OutfitResponse>>("/api/outfits/saved");

        Assert.Equal(["v4", "v3", "v2", "v1"], saved.Select(outfit => outfit.Id));
        Assert.All(saved, outfit => Assert.True(outfit.Saved));
    }

    [Fact]
    public async Task Gets_a_seeded_outfit_by_id()
    {
        var outfit = await Client.GetJsonAsync<OutfitResponse>("/api/outfits/v3");

        Assert.Equal("Wedding guest", outfit.Title);
        Assert.Equal("f1", outfit.Pieces.Outer);
        Assert.Equal("f3", outfit.Pieces.Top);
        Assert.Equal("f2", outfit.Pieces.Bottom);
        Assert.Equal("f5", outfit.Pieces.Shoes);
        Assert.Equal(["f4", "a1"], outfit.Pieces.Acc);
    }

    [Fact]
    public async Task Unknown_outfit_ids_are_404_for_every_outfit_route()
    {
        var responses = new[]
        {
            await Client.GetAsync("/api/outfits/ghost"),
            await Client.PostJsonAsync("/api/outfits/ghost/refine", new { instruction = "x" }),
            await Client.GetAsync("/api/outfits/ghost/alternatives?slot=shoes"),
            await Client.PutJsonAsync("/api/outfits/ghost/pieces/shoes", new { itemId = "s1" }),
            await Client.PostAsync("/api/outfits/ghost/reject", null),
            await Client.PutJsonAsync("/api/outfits/ghost/saved", new { saved = true }),
            await Client.PostAsync("/api/outfits/ghost/wears", null),
            await Client.PostAsync("/api/outfits/ghost/try-on", null),
        };

        Assert.All(responses, response =>
        {
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        });
    }

    [Fact]
    public async Task Today_is_the_seeded_outfit_and_stable_across_calls()
    {
        var first = await Client.GetJsonAsync<OutfitResponse>("/api/outfits/today");
        var second = await Client.GetJsonAsync<OutfitResponse>("/api/outfits/today");

        Assert.Equal(first.Id, second.Id);
        Assert.Equal("Monday, rain later", first.Title);
        Assert.Equal("o5", first.Pieces.Outer);
    }

    [Fact]
    public async Task Today_is_generated_on_a_new_day_then_stable_for_that_day()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();
        var seeded = await client.GetJsonAsync<OutfitResponse>("/api/outfits/today");

        factory.Clock.Advance(TimeSpan.FromDays(1));
        var tuesday = await client.GetJsonAsync<OutfitResponse>("/api/outfits/today");
        var again = await client.GetJsonAsync<OutfitResponse>("/api/outfits/today");

        Assert.NotEqual(seeded.Id, tuesday.Id);
        Assert.Equal(tuesday.Id, again.Id);
        Assert.Equal("Tuesday, rain later", tuesday.Title);
        Assert.Equal(tuesday.Id, (await client.GetJsonAsync<OutfitResponse>($"/api/outfits/{tuesday.Id}")).Id);
    }

    [Fact]
    public async Task Style_creates_an_outfit_built_only_from_closet_items()
    {
        var response = await Client.PostJsonAsync("/api/outfits/style", new StyleRequest { Prompt = "a wedding" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var outfit = await response.ReadAsync<OutfitResponse>();
        Assert.Equal($"http://localhost/api/outfits/{outfit.Id}", response.Headers.Location!.ToString());
        Assert.Equal("Formal", outfit.Title);
        Assert.Equal("a wedding", outfit.Prompt);
        var closetIds = (await Closet()).Select(item => item.Id).ToHashSet();
        Assert.All(new[] { outfit.Pieces.Top, outfit.Pieces.Bottom, outfit.Pieces.Shoes }.Concat(outfit.Pieces.Acc), id => Assert.Contains(id, closetIds));
        Assert.Equal(outfit.Id, (await Client.GetJsonAsync<OutfitResponse>($"/api/outfits/{outfit.Id}")).Id);
    }

    [Fact]
    public async Task Style_honours_the_locked_item()
    {
        var outfit = await (await Client.PostJsonAsync("/api/outfits/style", new StyleRequest { Prompt = "dinner", LockItemId = "s2" })).ReadAsync<OutfitResponse>();

        Assert.Equal("s2", outfit.Pieces.Shoes);
    }

    [Fact]
    public async Task Style_with_avoid_recent_uses_the_fresh_rotation_wording()
    {
        var outfit = await (await Client.PostJsonAsync("/api/outfits/style", new StyleRequest { Prompt = "surprise me", AvoidRecent = true })).ReadAsync<OutfitResponse>();

        Assert.StartsWith("Nothing here was worn in the last week.", outfit.Note);
        Assert.Equal("Something new", outfit.Title);
    }

    [Fact]
    public async Task Style_validates_the_request_and_the_locked_item()
    {
        var empty = await Client.PostAsJsonAsync("/api/outfits/style", new { prompt = "" });
        var missing = await Client.PostAsJsonAsync("/api/outfits/style", new { });
        var unknownLock = await Client.PostJsonAsync("/api/outfits/style", new StyleRequest { Prompt = "x", LockItemId = "ghost" });

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknownLock.StatusCode);
    }

    [Fact]
    public async Task Style_maps_ai_provider_failure_to_502_problem_json()
    {
        using var failing = new ApiFactory { Stylist = new ThrowingStylist() };
        var client = failing.CreateClient();

        var style = await client.PostJsonAsync("/api/outfits/style", new StyleRequest { Prompt = "x" });
        var refine = await client.PostJsonAsync("/api/outfits/v1/refine", new { instruction = "different shoes" });
        var reject = await client.PostAsync("/api/outfits/v1/reject", null);

        Assert.All(new[] { style, refine, reject }, response =>
        {
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        });
        Assert.Equal(502, (await style.ReadAsync<ProblemDetails>()).Status);
    }

    [Fact]
    public async Task Refine_with_a_slot_instruction_returns_a_new_outfit_with_only_that_slot_changed()
    {
        var original = await Client.GetJsonAsync<OutfitResponse>("/api/outfits/v1");

        var response = await Client.PostJsonAsync("/api/outfits/v1/refine", new { instruction = "different shoes" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var refined = await response.ReadAsync<OutfitResponse>();
        Assert.NotEqual(original.Id, refined.Id);
        Assert.NotEqual(original.Pieces.Shoes, refined.Pieces.Shoes);
        Assert.Equal(original.Pieces.Top, refined.Pieces.Top);
        Assert.Equal(original.Pieces.Bottom, refined.Pieces.Bottom);
        Assert.Equal(original.Title, refined.Title);
        Assert.Equal("s4", (await Client.GetJsonAsync<OutfitResponse>("/api/outfits/v1")).Pieces.Shoes);
    }

    [Fact]
    public async Task Refine_requires_an_instruction()
    {
        var response = await Client.PostJsonAsync("/api/outfits/v1/refine", new { instruction = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Alternatives_lists_closet_items_for_the_slot_excluding_the_current_piece()
    {
        var alternatives = await Client.GetJsonAsync<List<ItemResponse>>("/api/outfits/v1/alternatives?slot=shoes");

        Assert.NotEmpty(alternatives);
        Assert.All(alternatives, item => Assert.Equal(Slot.Shoes, item.Slot));
        Assert.DoesNotContain(alternatives, item => item.Id == "s4");
    }

    [Fact]
    public async Task Alternatives_requires_a_valid_slot()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.GetAsync("/api/outfits/v1/alternatives")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.GetAsync("/api/outfits/v1/alternatives?slot=hats")).StatusCode);
    }

    [Fact]
    public async Task Replace_piece_swaps_the_item_and_clears_saved()
    {
        var response = await Client.PutJsonAsync("/api/outfits/v1/pieces/shoes", new { itemId = "s1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var outfit = await response.ReadAsync<OutfitResponse>();
        Assert.Equal("s1", outfit.Pieces.Shoes);
        Assert.False(outfit.Saved);
        Assert.Equal("s1", (await Client.GetJsonAsync<OutfitResponse>("/api/outfits/v1")).Pieces.Shoes);
    }

    [Fact]
    public async Task Replace_piece_rejects_an_item_from_the_wrong_slot_with_400()
    {
        var response = await Client.PutJsonAsync("/api/outfits/v1/pieces/shoes", new { itemId = "t1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.ReadAsync<ValidationProblemDetails>();
        Assert.Contains("itemId", problem.Errors.Keys);
        Assert.Equal("s4", (await Client.GetJsonAsync<OutfitResponse>("/api/outfits/v1")).Pieces.Shoes);
    }

    [Fact]
    public async Task Replace_piece_with_an_unknown_item_is_404_and_an_unknown_slot_is_400()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await Client.PutJsonAsync("/api/outfits/v1/pieces/shoes", new { itemId = "ghost" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutJsonAsync("/api/outfits/v1/pieces/hat", new { itemId = "s1" })).StatusCode);
    }

    [Fact]
    public async Task Replace_accessory_keeps_the_watch()
    {
        var outfit = await (await Client.PutJsonAsync("/api/outfits/v1/pieces/acc", new { itemId = "a2" })).ReadAsync<OutfitResponse>();

        Assert.Equal(["a1", "a2"], outfit.Pieces.Acc);
    }

    [Fact]
    public async Task Reject_returns_a_replacement_outfit()
    {
        var original = await Client.GetJsonAsync<OutfitResponse>("/api/outfits/v1");

        var response = await Client.PostAsync("/api/outfits/v1/reject", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var replacement = await response.ReadAsync<OutfitResponse>();
        Assert.NotEqual(original.Id, replacement.Id);
        Assert.False(replacement.Saved);
    }

    [Fact]
    public async Task Saved_flag_can_be_toggled_and_listed()
    {
        var unsaved = await (await Client.PutJsonAsync("/api/outfits/v1/saved", new { saved = false })).ReadAsync<OutfitResponse>();
        var savedList = await Client.GetJsonAsync<List<OutfitResponse>>("/api/outfits/saved");
        var resaved = await (await Client.PutJsonAsync("/api/outfits/v1/saved", new { saved = true })).ReadAsync<OutfitResponse>();

        Assert.False(unsaved.Saved);
        Assert.DoesNotContain(savedList, outfit => outfit.Id == "v1");
        Assert.True(resaved.Saved);
        Assert.Contains((await Client.GetJsonAsync<List<OutfitResponse>>("/api/outfits/saved")), outfit => outfit.Id == "v1");
    }

    [Fact]
    public async Task Saved_requires_a_boolean()
    {
        Assert.Equal(HttpStatusCode.BadRequest, (await Client.PutJsonAsync("/api/outfits/v1/saved", new { })).StatusCode);
    }

    [Fact]
    public async Task Styling_an_empty_closet_is_a_400()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();
        await client.GetAsync("/api/items");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClosetAPI.Infrastructure.Persistence.ClosetDbContext>();
        db.Items.RemoveRange(db.Items);
        await db.SaveChangesAsync();

        var response = await client.PostJsonAsync("/api/outfits/style", new StyleRequest { Prompt = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
