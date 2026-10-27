using ClosetAPI.Domain.Outfits;

namespace Closet.Tests.Domain;

public class DressLevelTests
{
    [Theory]
    [InlineData("a black tie gala", 3, Mood.Formal)]
    [InlineData("job interview", 3, Mood.Formal)]
    [InlineData("office meeting", 2.4, Mood.Business)]
    [InlineData("saturday brunch", 1.1, Mood.Casual)]
    [InlineData("what should I wear", 2, Mood.Today)]
    [InlineData("drinks tonight", 1.9, Mood.Dinner)]
    [InlineData("casual dinner", 1.5, Mood.DinnerCasual)]
    [InlineData("formal dinner", 1.9, Mood.Dinner)]
    public void Reads_dress_level_from_the_prompt(string prompt, double target, Mood mood)
    {
        var level = DressLevel.FromPrompt(prompt);

        Assert.Equal(target, level.Target);
        Assert.Equal(mood, level.Mood);
    }
}
