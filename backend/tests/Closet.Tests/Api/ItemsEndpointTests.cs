using System.Net;
using System.Net.Http.Json;
using Closet.Tests.Support;
using ClosetAPI.Application.Dtos.Items.SuggestItem;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Domain.Items;
using Microsoft.AspNetCore.Mvc;

namespace Closet.Tests.Api;

public class ItemsEndpointTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Lists_the_seeded_closet_with_integer_formality()
    {
        var items = await Client.GetJsonAsync<List<ItemResponse>>("/api/items");

        Assert.Equal(34, items.Count);
        Assert.All(items, item => Assert.InRange(item.Formality, 1, 3));
        var watch = items.Single(item => item.Id == "a1");
        Assert.Equal(Slot.Acc, watch.Slot);
        Assert.Equal(212, watch.TimesWorn);
        Assert.Equal(new DateOnly(2026, 10, 25), watch.LastWorn);
    }

    [Fact]
    public async Task Creates_an_item_with_location_and_defaults()
    {
        var response = await Client.PostJsonAsync("/api/items", Fixtures.NewItem("Grey hoodie"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var item = await response.ReadAsync<ItemResponse>();
        Assert.Equal(0, item.TimesWorn);
        Assert.Null(item.LastWorn);
        Assert.Equal($"http://localhost/api/items/{item.Id}", response.Headers.Location!.ToString());
        Assert.Equal("Grey hoodie", (await Client.GetJsonAsync<ItemResponse>($"/api/items/{item.Id}")).Name);
        Assert.Equal(35, (await Client.GetJsonAsync<List<ItemResponse>>("/api/items")).Count);
    }

    [Fact]
    public async Task Persists_optional_fields()
    {
        var newItem = Fixtures.NewItem() with
        {
            Variant = new Dictionary<string, string> { ["stripe"] = "#fff" },
            Keywords = ["hoodie"],
            Seasons = ["Fall"],
            Price = 59.5m,
            RainOk = true,
            ImageUri = "http://x/y.png",
        };

        var created = await (await Client.PostJsonAsync("/api/items", newItem)).ReadAsync<ItemResponse>();
        var fetched = await Client.GetJsonAsync<ItemResponse>($"/api/items/{created.Id}");

        Assert.Equal("#fff", fetched.Variant!["stripe"]);
        Assert.Equal(["hoodie"], fetched.Keywords);
        Assert.Equal(["Fall"], fetched.Seasons);
        Assert.Equal(59.5m, fetched.Price);
        Assert.True(fetched.RainOk);
        Assert.Equal("http://x/y.png", fetched.ImageUri);
    }

    [Theory]
    [InlineData("spaceship", 1)]
    [InlineData("tee", 0)]
    [InlineData("tee", 4)]
    public async Task Rejects_unknown_garment_type_and_out_of_range_formality(string type, int formality)
    {
        var response = await Client.PostJsonAsync("/api/items", Fixtures.NewItem(type: type, formality: formality));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.ReadAsync<ValidationProblemDetails>();
        Assert.NotEmpty(problem.Errors);
    }

    [Fact]
    public async Task Rejects_a_body_with_missing_required_fields_and_unknown_enum_values()
    {
        var missing = await Client.PostAsJsonAsync("/api/items", new { name = "x" });
        var badEnum = await Client.PostAsJsonAsync("/api/items", new
        {
            name = "x", brand = "b", category = "hats", slot = "top", type = "tee", color = "#000", colorName = "c", formality = 1, fabric = "f", size = "m",
        });

        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, badEnum.StatusCode);
    }

    [Fact]
    public async Task Unknown_item_is_404_problem_json()
    {
        var response = await Client.GetAsync("/api/items/ghost");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(404, (await response.ReadAsync<ProblemDetails>()).Status);
    }

    [Fact]
    public async Task Analyze_returns_a_suggestion_and_serves_the_cutout_without_saving_an_item()
    {
        var response = await Client.PostAsync("/api/items/analyze", HttpClientJson.PhotoForm(TestImages.Png));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var suggestion = await response.ReadAsync<ItemSuggestion>();
        Assert.StartsWith("http://localhost/media/", suggestion.CutoutUri);
        Assert.True(GarmentTypes.IsKnown(suggestion.Type));
        Assert.Equal(34, (await Client.GetJsonAsync<List<ItemResponse>>("/api/items")).Count);

        var media = await Client.GetAsync(new Uri(suggestion.CutoutUri).AbsolutePath);
        Assert.Equal(HttpStatusCode.OK, media.StatusCode);
        Assert.Equal(TestImages.Png, await media.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Analyze_accepts_jpeg()
    {
        var response = await Client.PostAsync("/api/items/analyze", HttpClientJson.PhotoForm(TestImages.Jpeg, "a.jpg", "image/jpeg"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.EndsWith(".jpg", (await response.ReadAsync<ItemSuggestion>()).CutoutUri);
    }

    [Fact]
    public async Task Analyze_rejects_missing_wrong_type_and_oversized_photos()
    {
        var none = await Client.PostAsync("/api/items/analyze", new MultipartFormDataContent());
        var text = await Client.PostAsync("/api/items/analyze", HttpClientJson.PhotoForm("hello"u8.ToArray(), "a.txt", "text/plain"));
        var big = await Client.PostAsync("/api/items/analyze", HttpClientJson.PhotoForm(TestImages.Oversized()));

        Assert.Equal(HttpStatusCode.BadRequest, none.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, text.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, big.StatusCode);
        Assert.Equal("application/problem+json", big.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Analyze_maps_ai_provider_failure_to_502()
    {
        using var failing = new ApiFactory { Tagger = new ThrowingTagger() };

        var response = await failing.CreateClient().PostAsync("/api/items/analyze", HttpClientJson.PhotoForm(TestImages.Png));

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(502, (await response.ReadAsync<ProblemDetails>()).Status);
    }
}
