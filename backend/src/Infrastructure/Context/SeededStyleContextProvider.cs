using ClosetAPI.Domain.Context;

namespace ClosetAPI.Infrastructure.Context;

public class SeededStyleContextProvider(TimeProvider clock) : IStyleContextProvider
{
    private const int SeededTempF = 57;
    private const string SeededForecast = "Rain after 4pm";

    public StyleContext GetCurrentStyleContext()
    {
        var today = DateOnly.FromDateTime(clock.GetLocalNow().DateTime);
        return new StyleContext(today, today.DayOfWeek.ToString(), SeededTempF, SeededForecast, Rain: true);
    }
}
