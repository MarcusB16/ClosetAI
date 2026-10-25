using ClosetAPI.Application.Dtos.Wears.GetWornOutfits;
using ClosetAPI.Application.Dtos.Wears.LogWear;
using ClosetAPI.Application.Mapping;
using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Wears;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.OpenApi.Examples;
using ClosetAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api")]
[Tags("Wears")]
public class WearsController(ClosetDbContext db, IStyleContextProvider contextProvider) : ControllerBase
{
    [HttpPost("outfits/{id}/wears")]
    [EndpointSummary("Log wearing an outfit today")]
    [EndpointDescription("Records today's wear and increments `timesWorn` and `lastWorn` on every piece. Logging again on the same day replaces the earlier entry and undoes its increments.")]
    [ProducesResponseType<WearEntryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ResponseExample<WearEntryExample>(StatusCodes.Status201Created)]
    public async Task<ActionResult<WearEntryResponse>> Log(string id, CancellationToken cancellationToken)
    {
        var outfit = await db.GetOutfitAsync(id, cancellationToken);
        var today = contextProvider.Current().Date;
        var closet = await db.LoadClosetAsync(cancellationToken);

        var previous = await db.Wears.FirstOrDefaultAsync(wear => wear.Date == today, cancellationToken);
        var replacedOutfit = previous is null
            ? null
            : await db.Outfits.FirstOrDefaultAsync(candidate => candidate.Id == previous.OutfitId, cancellationToken);

        WearRecorder.Apply(today, outfit, replacedOutfit, closet);
        if (previous is null) db.Wears.Add(new WearEntry { Date = today, OutfitId = outfit.Id });
        else previous.OutfitId = outfit.Id;
        await db.SaveChangesAsync(cancellationToken);

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
        var wears = await db.Wears.ToListAsync(cancellationToken);
        var outfits = (await db.Outfits.ToListAsync(cancellationToken)).ToDictionary(outfit => outfit.Id);

        return wears
            .Where(wear => outfits.ContainsKey(wear.OutfitId))
            .OrderByDescending(wear => wear.Date)
            .Select(wear => new WornOutfit
            {
                Date = wear.Date,
                OutfitId = wear.OutfitId,
                Outfit = outfits[wear.OutfitId].ToResponse(),
            })
            .ToList();
    }
}
