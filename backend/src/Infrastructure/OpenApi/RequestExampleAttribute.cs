namespace ClosetAPI.Infrastructure.OpenApi;

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequestExampleAttribute<TExample> : ExampleAttribute where TExample : IOpenApiExample
{
    public override object ExampleValue => TExample.Value;
}
