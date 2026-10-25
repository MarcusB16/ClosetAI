using System.Text.RegularExpressions;

namespace ClosetAPI.Domain.Outfits;

public partial record DressLevel(double Target, Mood Mood)
{
    public static DressLevel FromPrompt(string prompt)
    {
        var text = prompt.ToLowerInvariant();
        var level = new DressLevel(2, Mood.Today);

        if (FormalPattern().IsMatch(text)) level = new DressLevel(3, Mood.Formal);
        else if (BusinessPattern().IsMatch(text)) level = new DressLevel(2.4, Mood.Business);
        else if (CasualPattern().IsMatch(text)) level = new DressLevel(1.1, Mood.Casual);

        if (!DinnerPattern().IsMatch(text)) return level;

        return level.Mood == Mood.Casual
            ? new DressLevel(1.5, Mood.DinnerCasual)
            : new DressLevel(Math.Min(level.Target, 1.9), Mood.Dinner);
    }

    [GeneratedRegex("formal|wedding|black tie|interview|suit|gala")]
    private static partial Regex FormalPattern();

    [GeneratedRegex("business|work|office|meeting|client|smart")]
    private static partial Regex BusinessPattern();

    [GeneratedRegex("casual|weekend|relaxed|errand|coffee|brunch|walk")]
    private static partial Regex CasualPattern();

    [GeneratedRegex("dinner|date|drinks")]
    private static partial Regex DinnerPattern();
}
