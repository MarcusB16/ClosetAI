using System.Net;
using Closet.Tests.Support;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Application.Dtos.TryOns.GetTryOn;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.TryOns;

namespace Closet.Tests.Api;

public class TryOnEndpointTests(ApiFactory factory) : ApiTestBase(factory)
{
    private Task<HttpResponseMessage> TryOn(string outfitId = "v1") => Client.PostAsync($"/api/outfits/{outfitId}/try-on", null);

    [Fact]
    public async Task Without_a_person_photo_the_status_is_needs_photo()
    {
        var response = await TryOn();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.ReadAsync<TryOnResult>();
        Assert.Equal(TryOnStatus.NeedsPhoto, result.Status);
        Assert.Null(result.ImageUri);
        Assert.Contains("\"status\":\"needsPhoto\"", await (await TryOn()).Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task With_a_photo_the_result_is_ready_and_flagged_as_placeholder()
    {
        await Client.PutAsync("/api/profile/person-photo", HttpClientJson.PhotoForm(TestImages.Png));

        var result = await (await TryOn()).ReadAsync<TryOnResult>();

        Assert.Equal(TryOnStatus.Ready, result.Status);
        Assert.True(result.Placeholder);
        Assert.StartsWith("http://localhost/media/", result.ImageUri);
        Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync(new Uri(result.ImageUri!).AbsolutePath)).StatusCode);
    }

    [Fact]
    public async Task Results_are_cached_per_outfit_pieces_and_person_photo()
    {
        var counter = new CountingRenderer();
        using var counting = new ApiFactory { Renderer = counter };
        var client = counting.CreateClient();
        await client.PutAsync("/api/profile/person-photo", HttpClientJson.PhotoForm(TestImages.Png));

        await client.PostAsync("/api/outfits/v1/try-on", null);
        await client.PostAsync("/api/outfits/v1/try-on", null);
        Assert.Equal(1, counter.Calls);

        await client.PutJsonAsync("/api/outfits/v1/pieces/shoes", new { itemId = "s1" });
        await client.PostAsync("/api/outfits/v1/try-on", null);
        Assert.Equal(2, counter.Calls);

        await client.PutAsync("/api/profile/person-photo", HttpClientJson.PhotoForm(TestImages.Jpeg, "b.jpg", "image/jpeg"));
        await client.PostAsync("/api/outfits/v1/try-on", null);
        Assert.Equal(3, counter.Calls);
    }

    [Fact]
    public async Task Renderer_failure_returns_a_failed_status_not_an_error()
    {
        using var failing = new ApiFactory { Renderer = new ThrowingRenderer() };
        var client = failing.CreateClient();
        await client.PutAsync("/api/profile/person-photo", HttpClientJson.PhotoForm(TestImages.Png));

        var response = await client.PostAsync("/api/outfits/v1/try-on", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.ReadAsync<TryOnResult>();
        Assert.Equal(TryOnStatus.Failed, result.Status);
        Assert.NotNull(result.Message);
    }
}
