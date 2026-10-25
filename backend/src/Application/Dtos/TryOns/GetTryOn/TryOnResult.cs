namespace ClosetAPI.Application.Dtos.TryOns.GetTryOn;

public record TryOnResult
{
    public required TryOnStatus Status { get; init; }
    public string? ImageUri { get; init; }
    public bool? Placeholder { get; init; }
    public string? Message { get; init; }
}
