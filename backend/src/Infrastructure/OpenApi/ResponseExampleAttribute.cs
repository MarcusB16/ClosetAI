namespace ClosetAPI.Infrastructure.OpenApi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ResponseExampleAttribute<TExample>(int statusCode) : ExampleAttribute where TExample : IOpenApiExample
{
    public override int? StatusCode { get; } = statusCode;

    public override object ExampleValue => TExample.Value;
}
