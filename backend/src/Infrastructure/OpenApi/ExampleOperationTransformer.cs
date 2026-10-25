using System.Text.Json;
using System.Text.Json.Nodes;
using ClosetAPI.Infrastructure.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ClosetAPI.Infrastructure.OpenApi;

public class ExampleOperationTransformer : IOpenApiOperationTransformer
{
    private const string JsonContentType = "application/json";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var examples = context.Description.ActionDescriptor.EndpointMetadata.OfType<ExampleAttribute>();
        foreach (var example in examples)
        {
            var mediaType = example.StatusCode is { } statusCode
                ? ResponseMediaType(operation, statusCode)
                : RequestMediaType(operation);
            if (mediaType is not null) mediaType.Example = ToNode(example);
        }
        return Task.CompletedTask;
    }

    private static OpenApiMediaType? RequestMediaType(OpenApiOperation operation)
    {
        OpenApiMediaType? mediaType = null;
        operation.RequestBody?.Content?.TryGetValue(JsonContentType, out mediaType);
        return mediaType;
    }

    private static OpenApiMediaType? ResponseMediaType(OpenApiOperation operation, int statusCode)
    {
        if (operation.Responses is null || !operation.Responses.TryGetValue(statusCode.ToString(), out var response)) return null;
        OpenApiMediaType? mediaType = null;
        response.Content?.TryGetValue(JsonContentType, out mediaType);
        return mediaType;
    }

    private static JsonNode? ToNode(ExampleAttribute example) =>
        JsonSerializer.SerializeToNode(example.ExampleValue, ApiJson.Options);
}
