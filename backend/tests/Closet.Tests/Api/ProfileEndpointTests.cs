using System.Net;
using Closet.Tests.Support;
using ClosetAPI.Application.Dtos.Shared;

namespace Closet.Tests.Api;

public class ProfileEndpointTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Starts_not_onboarded_without_a_photo()
    {
        var profile = await Client.GetJsonAsync<Profile>("/api/profile");

        Assert.False(profile.Onboarded);
        Assert.Null(profile.PersonPhotoUri);
    }

    [Fact]
    public async Task Patch_completes_onboarding_and_ignores_missing_fields()
    {
        var done = await (await Client.PatchJsonAsync("/api/profile", new { onboarded = true })).ReadAsync<Profile>();
        var untouched = await (await Client.PatchJsonAsync("/api/profile", new { })).ReadAsync<Profile>();

        Assert.True(done.Onboarded);
        Assert.True(untouched.Onboarded);
    }

    [Fact]
    public async Task Uploading_a_person_photo_stores_it_and_serves_it()
    {
        var response = await Client.PutAsync("/api/profile/person-photo", HttpClientJson.PhotoForm(TestImages.Jpeg, "me.jpg", "image/jpeg"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.ReadAsync<Profile>();
        Assert.StartsWith("http://localhost/media/", profile.PersonPhotoUri);
        var media = await Client.GetAsync(new Uri(profile.PersonPhotoUri!).AbsolutePath);
        Assert.Equal(TestImages.Jpeg, await media.Content.ReadAsByteArrayAsync());
        Assert.Equal(profile.PersonPhotoUri, (await Client.GetJsonAsync<Profile>("/api/profile")).PersonPhotoUri);
    }

    [Fact]
    public async Task Deleting_the_person_photo_clears_it()
    {
        await Client.PutAsync("/api/profile/person-photo", HttpClientJson.PhotoForm(TestImages.Png));

        var profile = await (await Client.DeleteAsync("/api/profile/person-photo")).ReadAsync<Profile>();

        Assert.Null(profile.PersonPhotoUri);
    }

    [Fact]
    public async Task Uploading_an_invalid_photo_is_a_400()
    {
        var response = await Client.PutAsync("/api/profile/person-photo", HttpClientJson.PhotoForm("nope"u8.ToArray(), "x.png"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
