using ClosetAPI.Infrastructure.Context;
using Microsoft.Extensions.Time.Testing;

namespace Closet.Tests.Infrastructure;

public class StyleContextProviderTests
{
    [Fact]
    public void Returns_the_clock_date_with_seeded_weather()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 10, 27, 9, 0, 0, TimeSpan.Zero));

        var context = new SeededStyleContextProvider(clock).GetCurrentStyleContext();

        Assert.Equal(new DateOnly(2026, 10, 27), context.Date);
        Assert.Equal("Tuesday", context.Weekday);
        Assert.Equal(57, context.TempF);
        Assert.True(context.Rain);
    }
}
