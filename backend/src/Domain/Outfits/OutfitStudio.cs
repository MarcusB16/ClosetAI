using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Persistence;
using ClosetAPI.Domain.Users;

namespace ClosetAPI.Domain.Outfits;

public class OutfitStudio(
    IOutfitRepository outfits,
    IItemRepository items,
    IUserStateRepository userStates,
    IUnitOfWork unitOfWork,
    IOutfitStylist stylist,
    IStyleContextProvider contextProvider,
    TimeProvider clock)
{
    public const string DefaultPrompt = "What should I wear today?";

    public async Task<Outfit> TodayAsync(CancellationToken cancellationToken)
    {
        var state = await userStates.GetOrCreateAsync(cancellationToken);
        var today = contextProvider.Current().Date;

        if (state.TodayOutfitDate == today && state.TodayOutfitId is { } existingId)
        {
            var existing = await outfits.FindAsync(existingId, cancellationToken);
            if (existing is not null) return existing;
        }

        var outfit = await CreateAsync(prompt: null, DefaultPrompt, lockItemId: null, avoidRecent: false, cancellationToken);
        state.TodayOutfitDate = today;
        state.TodayOutfitId = outfit.Id;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return outfit;
    }

    public async Task<Outfit> StyleAsync(string prompt, string? lockItemId, bool avoidRecent, CancellationToken cancellationToken)
    {
        if (lockItemId is not null) await items.GetAsync(lockItemId, cancellationToken);
        return await CreateAsync(prompt, prompt, lockItemId, avoidRecent, cancellationToken);
    }

    public async Task<Outfit> RejectAsync(Outfit rejected, CancellationToken cancellationToken)
    {
        var prompt = rejected.Prompt ?? DefaultPrompt;
        return await CreateAsync(rejected.Prompt, prompt, lockItemId: null, avoidRecent: false, cancellationToken);
    }

    public async Task<Outfit> RefineAsync(Outfit current, string instruction, CancellationToken cancellationToken)
    {
        var closet = await items.ListNewestFirstAsync(cancellationToken);
        var state = await userStates.GetOrCreateAsync(cancellationToken);
        state.StyleVariation++;

        var brief = new RefineBrief(current, instruction, closet, contextProvider.Current(), state.StyleVariation);
        var styled = await stylist.RefineAsync(brief, cancellationToken);

        var prompt = string.Join(' ', new[] { current.Prompt, instruction }.Where(part => !string.IsNullOrEmpty(part)));
        return await SaveAsync(styled, prompt, cancellationToken);
    }

    private async Task<Outfit> CreateAsync(string? prompt, string styledPrompt, string? lockItemId, bool avoidRecent, CancellationToken cancellationToken)
    {
        var closet = await items.ListNewestFirstAsync(cancellationToken);
        if (closet.Count == 0) throw new RuleViolationException("closet", "The closet is empty.");

        var state = await userStates.GetOrCreateAsync(cancellationToken);
        state.StyleVariation++;

        var brief = new StyleBrief(styledPrompt, lockItemId, avoidRecent, closet, contextProvider.Current(), state.StyleVariation);
        var styled = await stylist.StyleAsync(brief, cancellationToken);
        return await SaveAsync(styled, prompt, cancellationToken);
    }

    private async Task<Outfit> SaveAsync(StyledOutfit styled, string? prompt, CancellationToken cancellationToken)
    {
        var outfit = new Outfit
        {
            Id = EntityId.New(),
            Title = styled.Title,
            Note = styled.Note,
            Prompt = prompt,
            Pieces = styled.Pieces,
            Saved = false,
            CreatedAt = clock.GetUtcNow(),
        };
        outfits.Add(outfit);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return outfit;
    }
}
