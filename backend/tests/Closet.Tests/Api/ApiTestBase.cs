using System.Net;
using Closet.Tests.Support;

namespace Closet.Tests.Api;

public abstract class ApiTestBase(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    protected ApiFactory Factory { get; } = factory;
    protected HttpClient Client { get; } = factory.CreateClient();

    public async Task InitializeAsync()
    {
        var response = await Client.PostAsync("/api/demo/reset", null);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
