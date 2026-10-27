using System.Text.Json;
using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;

namespace ClosetAPI.Infrastructure.Ai;

public class ClaudeOutfitStylist(IStructuredLlm llm) : IOutfitStylist
{
    private const string SystemPrompt =
        "You are a personal stylist. Build one outfit using only items from the user's closet. " +
        "Pick exactly one top, one bottom and one shoes item, at most one outer layer, and any fitting accessories. " +
        "Respect the weather, the formality asked for, and any locked item. " +
        "When asked to avoid recent wears, skip items worn in the last 7 days. " +
        "The title is a short name for the outfit. The note is one or two friendly sentences explaining the choices.";

    public async Task<StyledOutfit> StyleOutfitAsync(StyleBrief brief, CancellationToken cancellationToken)
    {
        EnsureClosetCanBeStyled(brief.Closet);
        var request = new
        {
            task = "Build an outfit",
            prompt = brief.Prompt,
            lockedItemId = brief.LockItemId,
            avoidRecentlyWorn = brief.AvoidRecent,
            context = DescribeContext(brief.Context),
            closet = DescribeCloset(brief.Closet),
        };
        return await RequestOutfitFromClaudeAsync(request, brief.Closet, brief.LockItemId, cancellationToken);
    }

    public async Task<StyledOutfit> RefineOutfitAsync(RefineBrief brief, CancellationToken cancellationToken)
    {
        EnsureClosetCanBeStyled(brief.Closet);
        var request = new
        {
            task = "Change the current outfit as instructed. Keep every piece the instruction does not touch.",
            instruction = brief.Instruction,
            originalPrompt = brief.Current.Prompt,
            currentOutfit = new
            {
                title = brief.Current.Title,
                outer = brief.Current.Pieces.Outer,
                top = brief.Current.Pieces.Top,
                bottom = brief.Current.Pieces.Bottom,
                shoes = brief.Current.Pieces.Shoes,
                acc = brief.Current.Pieces.Acc,
            },
            context = DescribeContext(brief.Context),
            closet = DescribeCloset(brief.Closet),
        };
        return await RequestOutfitFromClaudeAsync(request, brief.Closet, requiredItemId: null, cancellationToken);
    }

    private async Task<StyledOutfit> RequestOutfitFromClaudeAsync(object request, IReadOnlyList<Item> closet, string? requiredItemId, CancellationToken cancellationToken)
    {
        var structured = new StructuredRequest(
            SystemPrompt,
            JsonSerializer.Serialize(request, ClosetJson.Options),
            OutfitSchema.ForCloset(closet));
        var json = await llm.CompleteWithJsonSchemaAsync(structured, cancellationToken);
        return OutfitResponseReader.ReadOutfit(json, closet, requiredItemId);
    }

    private static void EnsureClosetCanBeStyled(IReadOnlyList<Item> closet)
    {
        foreach (var slot in new[] { Slot.Top, Slot.Bottom, Slot.Shoes })
        {
            if (closet.All(item => item.Slot != slot))
                throw new RuleViolationException("closet", $"The closet has no items for the '{slot}' slot.");
        }
    }

    private static object DescribeContext(StyleContext context) => new
    {
        date = context.Date.ToString("yyyy-MM-dd"),
        weekday = context.Weekday,
        tempF = context.TempF,
        forecast = context.Forecast,
        rain = context.Rain,
    };

    private static IEnumerable<object> DescribeCloset(IReadOnlyList<Item> closet) => closet.Select(item => new
    {
        id = item.Id,
        name = item.Name,
        slot = item.Slot.ToString().ToLowerInvariant(),
        type = item.Type,
        colorName = item.ColorName,
        formality = item.Formality,
        lastWorn = item.LastWorn?.ToString("yyyy-MM-dd"),
        rainOk = item.RainOk,
        avoidInRain = item.NoRain,
        seasons = item.Seasons,
    });
}
