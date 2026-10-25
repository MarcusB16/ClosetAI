using ClosetAPI.Application.Dtos.Outfits.GenerateOutfit;
using ClosetAPI.Application.Dtos.Outfits.RefineOutfit;
using ClosetAPI.Application.Dtos.Outfits.ReplacePiece;
using ClosetAPI.Application.Dtos.Outfits.SetSaved;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Application.Mapping;
using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.OpenApi.Examples;
using ClosetAPI.Infrastructure.Persistence;
using ClosetAPI.Infrastructure.Styling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api/outfits")]
[Tags("Outfits")]
public class OutfitsController(
    ClosetDbContext db,
    OutfitStudio studio,
    IStyleContextProvider contextProvider) : ControllerBase
{
    [HttpGet("today")]
    [EndpointSummary("Get today's outfit")]
    [EndpointDescription("Generated on the first call each day, then stable for the rest of that day.")]
    [ProducesResponseType<OutfitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, "application/problem+json")]
    [ResponseExample<OutfitExample>(StatusCodes.Status200OK)]
    public async Task<OutfitResponse> Today(CancellationToken cancellationToken) =>
        (await studio.TodayAsync(cancellationToken)).ToResponse();

    [HttpGet("saved")]
    [EndpointSummary("List saved outfits")]
    [EndpointDescription("Returns outfits the user saved, newest first.")]
    [ProducesResponseType<List<OutfitResponse>>(StatusCodes.Status200OK)]
    [ResponseExample<OutfitListExample>(StatusCodes.Status200OK)]
    public async Task<List<OutfitResponse>> Saved(CancellationToken cancellationToken)
    {
        var saved = await db.Outfits.Where(outfit => outfit.Saved).ToListAsync(cancellationToken);
        return saved.OrderByDescending(outfit => outfit.CreatedAt).Select(outfit => outfit.ToResponse()).ToList();
    }

    [HttpGet("{id}")]
    [EndpointSummary("Get an outfit")]
    [ProducesResponseType<OutfitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ResponseExample<OutfitExample>(StatusCodes.Status200OK)]
    public async Task<OutfitResponse> Get(string id, CancellationToken cancellationToken) =>
        (await db.GetOutfitAsync(id, cancellationToken)).ToResponse();

    [HttpPost("style")]
    [EndpointSummary("Style a new outfit")]
    [EndpointDescription("Builds an outfit from the closet for a free-text request. Optionally lock one item in, or avoid pieces worn in the last week. Uses Claude when an API key is configured, otherwise a deterministic rules stylist.")]
    [Consumes("application/json")]
    [ProducesResponseType<OutfitResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, "application/problem+json")]
    [RequestExample<StyleRequestExample>]
    [ResponseExample<OutfitExample>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OutfitResponse>> Style(StyleRequest request, CancellationToken cancellationToken)
    {
        var outfit = await studio.StyleAsync(request.Prompt, request.LockItemId, request.AvoidRecent ?? false, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = outfit.Id }, outfit.ToResponse());
    }

    [HttpPost("{id}/refine")]
    [EndpointSummary("Refine an outfit")]
    [EndpointDescription("Applies a free-text change such as \"different shoes\" and returns the result as a new outfit. The original is left untouched.")]
    [Consumes("application/json")]
    [ProducesResponseType<OutfitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, "application/problem+json")]
    [RequestExample<RefineRequestExample>]
    [ResponseExample<OutfitExample>(StatusCodes.Status200OK)]
    public async Task<OutfitResponse> Refine(string id, RefineRequest request, CancellationToken cancellationToken)
    {
        var current = await db.GetOutfitAsync(id, cancellationToken);
        return (await studio.RefineAsync(current, request.Instruction, cancellationToken)).ToResponse();
    }

    [HttpGet("{id}/alternatives")]
    [EndpointSummary("List alternatives for a slot")]
    [EndpointDescription("Closet items that fit the given slot and the outfit's formality, excluding the current piece. Pass the slot as a query parameter, for example `?slot=shoes`.")]
    [ProducesResponseType<List<ItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ResponseExample<ItemListExample>(StatusCodes.Status200OK)]
    public async Task<List<ItemResponse>> Alternatives(string id, [FromQuery] Slot? slot, CancellationToken cancellationToken)
    {
        if (slot is null) throw new RuleViolationException("slot", "The slot query parameter is required.");

        var outfit = await db.GetOutfitAsync(id, cancellationToken);
        var closet = await db.LoadClosetAsync(cancellationToken);
        return AlternativeFinder.Find(outfit.Pieces, slot.Value, closet, contextProvider.Current())
            .Select(item => item.ToResponse())
            .ToList();
    }

    [HttpPut("{id}/pieces/{slot}")]
    [EndpointSummary("Swap one piece")]
    [EndpointDescription("Replaces the piece in a slot with another closet item and clears the saved flag. Returns 400 if the item does not belong to that slot and 404 if the outfit or item does not exist.")]
    [Consumes("application/json")]
    [ProducesResponseType<OutfitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [RequestExample<ReplacePieceRequestExample>]
    [ResponseExample<OutfitExample>(StatusCodes.Status200OK)]
    public async Task<OutfitResponse> ReplacePiece(string id, Slot slot, ReplacePieceRequest request, CancellationToken cancellationToken)
    {
        var outfit = await db.GetOutfitAsync(id, cancellationToken);
        var replacement = await db.GetItemAsync(request.ItemId, cancellationToken);
        var closet = await db.LoadClosetAsync(cancellationToken);

        outfit.Pieces = PieceReplacer.Replace(outfit.Pieces, slot, replacement, closet);
        outfit.Saved = false;
        await db.SaveChangesAsync(cancellationToken);
        return outfit.ToResponse();
    }

    [HttpPost("{id}/reject")]
    [EndpointSummary("Reject an outfit")]
    [EndpointDescription("Generates and returns a replacement outfit for the same request.")]
    [ProducesResponseType<OutfitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, "application/problem+json")]
    [ResponseExample<OutfitExample>(StatusCodes.Status200OK)]
    public async Task<OutfitResponse> Reject(string id, CancellationToken cancellationToken)
    {
        var rejected = await db.GetOutfitAsync(id, cancellationToken);
        return (await studio.RejectAsync(rejected, cancellationToken)).ToResponse();
    }

    [HttpPut("{id}/saved")]
    [EndpointSummary("Save or unsave an outfit")]
    [Consumes("application/json")]
    [ProducesResponseType<OutfitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [RequestExample<SetSavedRequestExample>]
    [ResponseExample<OutfitExample>(StatusCodes.Status200OK)]
    public async Task<OutfitResponse> SetSaved(string id, SetSavedRequest request, CancellationToken cancellationToken)
    {
        var outfit = await db.GetOutfitAsync(id, cancellationToken);
        outfit.Saved = request.Saved;
        await db.SaveChangesAsync(cancellationToken);
        return outfit.ToResponse();
    }
}
