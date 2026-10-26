using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using ClosetAPI.Domain.Errors;
using Microsoft.Extensions.Options;

namespace ClosetAPI.Infrastructure.Ai;

public class AnthropicStructuredLlm(AnthropicClient client, IOptions<AnthropicOptions> options) : IStructuredLlm
{
    private const int MaxTokens = 8000;

    public async Task<JsonElement> CompleteWithJsonSchemaAsync(StructuredRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await client.Messages.Create(BuildParameters(request), cancellationToken);
            return ReadJson(response);
        }
        catch (Exception exception) when (exception is not OperationCanceledException and not AiProviderException)
        {
            throw new AiProviderException($"The AI provider call failed: {exception.Message}", exception);
        }
    }

    private MessageCreateParams BuildParameters(StructuredRequest request) => new()
    {
        Model = options.Value.Model,
        MaxTokens = MaxTokens,
        System = request.SystemPrompt,
        Messages = [new UserMessageParam { Content = BuildContent(request) }],
        OutputConfig = new OutputConfig
        {
            Effort = Effort.Medium,
            Format = new JsonOutputFormat { Schema = ToSchemaDictionary(request) },
        },
    };

    private static List<ContentBlockParam> BuildContent(StructuredRequest request)
    {
        var content = new List<ContentBlockParam>();
        if (request.Image is not null)
        {
            content.Add(new ImageBlockParam
            {
                Source = new Base64ImageSource
                {
                    Data = Convert.ToBase64String(request.Image.Bytes),
                    MediaType = request.Image.ContentType == "image/png" ? MediaType.ImagePng : MediaType.ImageJpeg,
                },
            });
        }
        content.Add(new TextBlockParam { Text = request.UserText });
        return content;
    }

    private static Dictionary<string, JsonElement> ToSchemaDictionary(StructuredRequest request) =>
        request.Schema.ToDictionary(
            property => property.Key,
            property => JsonSerializer.SerializeToElement(property.Value));

    private static JsonElement ReadJson(Message response)
    {
        if (response.StopReason == StopReason.Refusal)
            throw new AiProviderException("The AI provider declined the request.");

        var text = response.Content.Select(block => block.Value).OfType<TextBlock>().FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
            throw new AiProviderException("The AI provider returned no content.");

        try
        {
            return JsonDocument.Parse(text).RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new AiProviderException("The AI provider returned invalid JSON.", exception);
        }
    }
}
