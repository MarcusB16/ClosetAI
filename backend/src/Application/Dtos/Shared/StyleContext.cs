namespace ClosetAPI.Application.Dtos.Shared;

public record StyleContext
{
    public required DateOnly Date { get; init; }
    public required string Weekday { get; init; }
    public required int TempF { get; init; }
    public required string Forecast { get; init; }
    public required bool Rain { get; init; }
}
