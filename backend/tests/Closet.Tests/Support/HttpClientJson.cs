using System.Net.Http.Json;
using ClosetAPI.Infrastructure.Http;

namespace Closet.Tests.Support;

public static class HttpClientJson
{
    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PostAsJsonAsync(url, body, ApiJson.Options);

    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PutAsJsonAsync(url, body, ApiJson.Options);

    public static Task<HttpResponseMessage> PatchJsonAsync<T>(this HttpClient client, string url, T body) =>
        client.PatchAsJsonAsync(url, body, ApiJson.Options);

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(ApiJson.Options);
        return value ?? throw new InvalidOperationException("Empty response body.");
    }

    public static async Task<T> GetJsonAsync<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsync<T>();
    }

    public static MultipartFormDataContent PhotoForm(byte[] bytes, string fileName = "photo.png", string contentType = "image/png")
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "photo", fileName } };
    }
}
