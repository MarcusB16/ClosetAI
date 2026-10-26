using ClosetAPI.Application.Dtos.Wears.GetWornOutfits;
using ClosetAPI.Application.Dtos.Wears.LogWear;
using ClosetAPI.Application.Mapping;
using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.Persistence;
using ClosetAPI.Domain.Wears;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.OpenApi.Examples;
using Microsoft.AspNetCore.Mvc;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api")]
[Tags("Wears")]
public class WearsController(
    IWearRepository wears,
    IOutfitRepository outfits,
    IItemRepository items,
    IUnitOfWork unitOfWork,
    IStyleContextProvider contextProvider) : ControllerBase
{
    [HttpPost("outfits/{id}/wears")]
    [EndpointSummary("Log wearing an outfit today")]
    [EndpointDescription("Records today's wear and increments `timesWorn` and `lastWorn` on every piece. Logging again on the same day replaces the earlier entry and undoes its increments.")]
    [ProducesResponseType<WearEntryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ResponseExample<WearEntryExample>(StatusCodes.Status201Created)]
    public async Task<ActionResult<WearEntryResponse>> LogWear(string id, CancellationToken cancellationToken)
    {
        var outfit = await outfits.GetAsync(id, cancellationToken);
        var today = contextProvider.GetCurrentStyleContext().Date;
        var closet = await items.ListNewestFirstAsync(cancellationToken);

        var previous = await wears.FindByDateAsync(today, cancellationToken);
        var replacedOutfit = previous is null ? null : await outfits.FindAsync(previous.OutfitId, cancellationToken);

        WearRecorder.RecordWear(today, outfit, replacedOutfit, closet);
        if (previous is null) wears.Add(new WearEntry { Date = today, OutfitId = outfit.Id });
        else previous.OutfitId = outfit.Id;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new WearEntryResponse { Date = today, OutfitId = outfit.Id };
        return Created("/api/wears", response);
    }

    [HttpGet("wears")]
    [EndpointSummary("List worn outfits")]
    [EndpointDescription("Wear history with the full outfit, newest first.")]
    [ProducesResponseType<List<WornOutfit>>(StatusCodes.Status200OK)]
    [ResponseExample<WornOutfitListExample>(StatusCodes.Status200OK)]
    public async Task<List<WornOutfit>> List(CancellationToken cancellationToken)
    {
        var history = await wears.ListNewestFirstAsync(cancellationToken);
        var wornIds = history.Select(wear => wear.OutfitId).Distinct().ToList();
        var wornOutfits = (await outfits.ListByIdsAsync(wornIds, cancellationToken)).ToDictionary(outfit => outfit.Id);

        return history
            .Where(wear => wornOutfits.ContainsKey(wear.OutfitId))
            .Select(wear => new WornOutfit
            {
                Date = wear.Date,
                OutfitId = wear.OutfitId,
                Outfit = wornOutfits[wear.OutfitId].ToResponse(),
            })
            .ToList();
    }
}
