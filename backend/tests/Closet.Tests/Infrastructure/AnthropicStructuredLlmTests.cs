using System.Net;
using System.Net.Sockets;
using System.Text;
using Anthropic;
using Closet.Tests.Support;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Media;
using ClosetAPI.Infrastructure.Ai;
using Microsoft.Extensions.Options;

namespace Closet.Tests.Infrastructure;

public class AnthropicStructuredLlmTests
{
    private static StructuredRequest Request(PhotoContent? image = null) =>
        new("system", "user", OutfitSchema.For(TestClosets.Small()), image);

    private static AnthropicStructuredLlm Llm(string baseUrl) => new(
        new AnthropicClient { ApiKey = "sk-ant-test", BaseUrl = baseUrl, MaxRetries = 0 },
        Options.Create(new AnthropicOptions { ApiKey = "sk-ant-test" }));

    [Fact]
    public async Task Unreachable_provider_becomes_an_ai_provider_exception()
    {
        var llm = Llm("http://127.0.0.1:1");

        await Assert.ThrowsAsync<AiProviderException>(() => llm.CompleteAsync(Request(), CancellationToken.None));
    }

    [Theory]
    [InlineData(500)]
    [InlineData(429)]
    [InlineData(401)]
    public async Task Provider_error_statuses_become_ai_provider_exceptions(int status)
    {
        using var server = StubServer.Respond(status, """{"type":"error","error":{"type":"api_error","message":"nope"}}""");

        await Assert.ThrowsAsync<AiProviderException>(() =>
            Llm(server.Url).CompleteAsync(Request(new PhotoContent(TestImages.Png, "image/png")), CancellationToken.None));
    }

    [Fact]
    public async Task Reads_structured_json_from_the_text_block_and_sends_the_image()
    {
        using var server = StubServer.Respond(200, MessageJson("""{\"title\":\"Hi\"}"""));

        var json = await Llm(server.Url).CompleteAsync(Request(new PhotoContent(TestImages.Jpeg, "image/jpeg")), CancellationToken.None);

        Assert.Equal("Hi", json.GetProperty("title").GetString());
        Assert.Contains("\"type\":\"image\"", server.LastRequestBody);
        Assert.Contains("image/jpeg", server.LastRequestBody);
        Assert.Contains("claude-sonnet-5-5", server.LastRequestBody);
        Assert.Contains("json_schema", server.LastRequestBody);
    }

    [Fact]
    public async Task Invalid_json_text_is_an_ai_provider_exception()
    {
        using var server = StubServer.Respond(200, MessageJson("not json"));

        await Assert.ThrowsAsync<AiProviderException>(() => Llm(server.Url).CompleteAsync(Request(), CancellationToken.None));
    }

    [Fact]
    public async Task Refusals_are_an_ai_provider_exception()
    {
        using var server = StubServer.Respond(200, MessageJson("", stopReason: "refusal"));

        await Assert.ThrowsAsync<AiProviderException>(() => Llm(server.Url).CompleteAsync(Request(), CancellationToken.None));
    }

    private static string MessageJson(string text, string stopReason = "end_turn") =>
        $$$"""
        {"id":"msg_1","type":"message","role":"assistant","model":"claude-sonnet-5-5","content":[{"type":"text","text":"{{{text}}}"}],"stop_reason":"{{{stopReason}}}","stop_sequence":null,"usage":{"input_tokens":1,"output_tokens":1}}
        """;
}
