using ClosetAPI.Application.Dtos.Shared;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class StyleContextExample : IOpenApiExample
{
    public static object Value => new StyleContext
    {
        Date = new DateOnly(2026, 10, 26),
        Weekday = "Monday",
        TempF = 57,
        Forecast = "Rain after 4pm",
        Rain = true,
    };
}
