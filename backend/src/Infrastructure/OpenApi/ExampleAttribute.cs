namespace ClosetAPI.Infrastructure.OpenApi;

public abstract class ExampleAttribute : Attribute
{
    public abstract object ExampleValue { get; }
    public virtual int? StatusCode => null;
}
