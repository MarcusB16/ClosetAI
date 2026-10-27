using System.Text.RegularExpressions;
using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Domain.Outfits;

public partial class RulesOutfitStylist : IOutfitStylist
{
    private const int RecentWearDays = 7;
    private const double FormalOnlyAbove = 2.8;

    public Task<StyledOutfit> StyleOutfitAsync(StyleBrief brief, CancellationToken cancellationToken) =>
        Task.FromResult(StyleByRules(brief));

    public Task<StyledOutfit> RefineOutfitAsync(RefineBrief brief, CancellationToken cancellationToken) =>
        Task.FromResult(RefineByRules(brief));

    public static Item? FindMentionedItem(string text, IReadOnlyList<Item> closet)
    {
        var lowered = text.ToLowerInvariant();
        return closet.FirstOrDefault(item =>
            lowered.Contains(item.Name.ToLowerInvariant())
            || (item.Keywords ?? []).Any(keyword => lowered.Contains(keyword)));
    }

    private static StyledOutfit RefineByRules(RefineBrief brief)
    {
        var slot = SlotInstructionParser.ParseSlot(brief.Instruction);
        if (slot is null) return StyleWithInstruction(brief);

        var alternatives = AlternativeFinder.FindAlternatives(brief.Current.Pieces, slot.Value, brief.Closet, brief.Context);
        if (alternatives.Count == 0) return KeepOutfitUnchanged(brief);

        var random = new SeededRandom(brief.Variation * 7919);
        var chosen = alternatives[random.NextIndex(alternatives.Count)];
        var pieces = PieceReplacer.ReplacePiece(brief.Current.Pieces, slot.Value, chosen, brief.Closet);
        return new StyledOutfit(brief.Current.Title, brief.Current.Note ?? "", pieces);
    }

    private static StyledOutfit KeepOutfitUnchanged(RefineBrief brief) =>
        new(brief.Current.Title, brief.Current.Note ?? "", brief.Current.Pieces);

    private static StyledOutfit StyleWithInstruction(RefineBrief brief)
    {
        var prompt = string.Join(' ', new[] { brief.Current.Prompt, brief.Instruction }.Where(part => !string.IsNullOrEmpty(part)));
        return StyleByRules(new StyleBrief(prompt, null, false, brief.Closet, brief.Context, brief.Variation));
    }

    private static StyledOutfit StyleByRules(StyleBrief brief)
    {
        var text = brief.Prompt.ToLowerInvariant();
        var random = new SeededRandom(brief.Variation * 7919);
        var level = DressLevel.FromPrompt(text);
        var closet = brief.Closet;

        var locked = brief.LockItemId is null
            ? FindMentionedItem(text, closet)
            : closet.FirstOrDefault(item => item.Id == brief.LockItemId);
        var target = locked is not null && level.Mood == Mood.Today ? locked.Formality : level.Target;
        var avoidRecent = brief.AvoidRecent || AvoidRecentPattern().IsMatch(text);

        bool IsAllowed(Item item) =>
            !(avoidRecent && item.Id != locked?.Id && !item.IsWatch && WasWornRecently(item, brief.Context.Date))
            && !(brief.Context.Rain && item.NoRain == true)
            && !(item.Category == Category.Formal && target < FormalOnlyAbove);

        Item? Pick(Slot slot)
        {
            if (locked is not null && locked.Slot == slot) return locked;
            var inSlot = closet.Where(item => item.Slot == slot).ToList();
            var candidates = inSlot.Where(item => IsAllowed(item) && Math.Abs(item.Formality - target) <= 1).ToList();
            if (candidates.Count == 0) candidates = inSlot.Where(IsAllowed).ToList();
            if (candidates.Count == 0) candidates = inSlot;
            if (candidates.Count == 0) return null;

            var weights = candidates.Select(item => ScoreCandidate(item, slot, target, brief.Context)).ToList();
            var roll = random.NextDouble() * weights.Sum();
            for (var index = 0; index < candidates.Count; index++)
            {
                roll -= weights[index];
                if (roll <= 0) return candidates[index];
            }
            return candidates[^1];
        }

        var outer = Pick(Slot.Outer);
        var top = Pick(Slot.Top) ?? throw MissingSlotError(Slot.Top);
        var bottom = Pick(Slot.Bottom) ?? throw MissingSlotError(Slot.Bottom);
        var shoes = Pick(Slot.Shoes) ?? throw MissingSlotError(Slot.Shoes);

        var accessories = new List<string>();
        var watch = closet.FirstOrDefault(item => item.IsWatch);
        if (watch is not null) accessories.Add(watch.Id);

        if (locked is { Slot: Slot.Acc } && locked.Id != watch?.Id) accessories.Add(locked.Id);
        else
        {
            var types = AccessoryTypesFor(target);
            var pool = closet.Where(item => item.Slot == Slot.Acc && types.Contains(item.Type) && IsAllowed(item)).ToList();
            if (pool.Count > 0) accessories.Add(pool[random.NextIndex(pool.Count)].Id);
        }

        var pieces = new OutfitPieces { Outer = outer?.Id, Top = top.Id, Bottom = bottom.Id, Shoes = shoes.Id, Acc = accessories };
        return new StyledOutfit(BuildTitle(locked, level.Mood, avoidRecent, brief.Context), BuildNote(locked, level.Mood, avoidRecent, outer, brief.Context), pieces);
    }

    private static double ScoreCandidate(Item item, Slot slot, double target, StyleContext context)
    {
        var closeness = 1 / (0.3 + Math.Abs(item.Formality - target));
        var formalBoost = target >= FormalOnlyAbove && item.Category == Category.Formal ? 5 : 1;
        var rainBoost = slot == Slot.Outer && context.Rain && item.RainOk == true ? 1.6 : 1;
        return closeness * formalBoost * rainBoost;
    }

    private static bool WasWornRecently(Item item, DateOnly today) =>
        item.LastWorn is { } lastWorn && today.DayNumber - lastWorn.DayNumber <= RecentWearDays;

    private static string[] AccessoryTypesFor(double target) => target switch
    {
        >= 2.8 => ["tie"],
        >= 2.2 => ["belt", "scarf"],
        >= 1.6 => ["belt", "scarf", "tote"],
        _ => ["beanie", "tote", "scarf", "belt"],
    };

    private static RuleViolationException MissingSlotError(Slot slot) =>
        new("closet", $"The closet has no items for the '{slot}' slot.");

    private static string LowercaseName(Item item) => char.ToLowerInvariant(item.Name[0]) + item.Name[1..];

    private static string BuildTitle(Item? locked, Mood mood, bool avoidRecent, StyleContext context)
    {
        if (locked is not null)
            return $"Around the {LeadingColorPattern().Replace(LowercaseName(locked), "")}";

        return mood switch
        {
            Mood.Formal => "Formal",
            Mood.Business => "Business casual",
            Mood.DinnerCasual => "Casual dinner",
            Mood.Dinner => "Dinner",
            Mood.Casual => "Easy casual",
            _ => avoidRecent ? "Something new" : $"{context.Weekday}{(context.Rain ? ", rain later" : "")}",
        };
    }

    private static string BuildNote(Item? locked, Mood mood, bool avoidRecent, Item? outer, StyleContext context)
    {
        var lead = locked is not null ? $"Built around your {LowercaseName(locked)}."
            : avoidRecent ? "Nothing here was worn in the last week."
            : mood switch
            {
                Mood.Formal => "Sharp and simple.",
                Mood.Business => "Polished enough for a meeting, easy enough for the rest of the day.",
                Mood.DinnerCasual => "Relaxed, one step up from daytime.",
                Mood.Dinner => "A little sharper for the evening.",
                Mood.Casual => "Easy pieces that still go together.",
                _ => "",
            };

        var weather = outer is null ? ""
            : context.Rain && outer.RainOk == true ? $"{context.Forecast}, so the {LowercaseName(outer)} goes on top."
            : $"It's {context.TempF}° today, so bring the {LowercaseName(outer)}.";

        return string.Join(' ', new[] { lead, weather }.Where(part => part.Length > 0));
    }

    [GeneratedRegex("haven.?t|different|recent|fresh|change it up|rotation")]
    private static partial Regex AvoidRecentPattern();

    [GeneratedRegex("^(white|black|brown|dark|light) ")]
    private static partial Regex LeadingColorPattern();
}
