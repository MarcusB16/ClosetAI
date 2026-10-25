using System.Net;
using Closet.Tests.Support;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Infrastructure.Http;

namespace Closet.Tests.Api;

public class ContextAndDemoTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Context_returns_the_clock_date_and_seeded_weather()
    {
        var context = await Client.GetJsonAsync<StyleContext>("/api/context");

        Assert.Equal(new DateOnly(2026, 10, 26), context.Date);
        Assert.Equal("Monday", context.Weekday);
        Assert.Equal(57, context.TempF);
        Assert.Equal("Rain after 4pm", context.Forecast);
        Assert.True(context.Rain);
    }

    [Fact]
    public async Task Context_json_uses_camel_case_and_iso_date()
    {
        var raw = await Client.GetStringAsync("/api/context");

        Assert.Contains("\"tempF\":57", raw);
        Assert.Contains("\"date\":\"2026-10-26\"", raw);
    }

    [Fact]
    public async Task Reset_restores_the_seeded_closet_after_changes()
    {
        await Client.PostJsonAsync("/api/items", Fixtures.NewItem("Extra tee"));
        await Client.PatchJsonAsync("/api/profile", new { onboarded = true });

        var response = await Client.PostAsync("/api/demo/reset", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(34, (await Client.GetJsonAsync<List<ItemResponse>>("/api/items")).Count);
        Assert.False((await Client.GetJsonAsync<Profile>("/api/profile")).Onboarded);
    }

    [Fact]
    public async Task Reset_is_not_available_outside_development()
    {
        using var production = new ApiFactory { EnvironmentName = "Production" };

        var response = await production.CreateClient().PostAsync("/api/demo/reset", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Openapi_document_and_swagger_ui_are_served_in_development_only()
    {
        Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync("/openapi/v1.json")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync("/swagger/index.html")).StatusCode);

        using var production = new ApiFactory { EnvironmentName = "Production" };
        Assert.Equal(HttpStatusCode.NotFound, (await production.CreateClient().GetAsync("/openapi/v1.json")).StatusCode);
    }

    [Fact]
    public async Task Unknown_routes_return_problem_json()
    {
        var response = await Client.GetAsync("/api/nope");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public void Api_json_does_not_add_a_second_enum_converter()
    {
        var options = ApiJson.Configure(new System.Text.Json.JsonSerializerOptions());
        ApiJson.Configure(options);

        Assert.Single(options.Converters.OfType<System.Text.Json.Serialization.JsonStringEnumConverter>());
    }
}
