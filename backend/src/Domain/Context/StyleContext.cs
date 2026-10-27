namespace ClosetAPI.Domain.Context;

public record StyleContext(DateOnly Date, string Weekday, int TempF, string Forecast, bool Rain);
