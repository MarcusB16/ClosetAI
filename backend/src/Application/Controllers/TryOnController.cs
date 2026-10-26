using ClosetAPI.Application.Dtos.TryOns.GetTryOn;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.Persistence;
using ClosetAPI.Domain.TryOns;
using ClosetAPI.Domain.Users;
using ClosetAPI.Infrastructure.Media;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.OpenApi.Examples;
using Microsoft.AspNetCore.Mvc;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api/outfits/{id}/try-on")]
[Tags("Try-on")]
public class TryOnController(
    IOutfitRepository outfits,
    IItemRepository items,
    IUserStateRepository userStates,
    ITryOnRenderRepository tryOnRenders,
    IUnitOfWork unitOfWork,
    ITryOnRenderer renderer,
    MediaUrlResolver mediaUrls,
    TimeProvider clock,
    ILogger<TryOnController> logger) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Try an outfit on")]
    [EndpointDescription("Renders the outfit on the profile's person photo. Returns status `needsPhoto` when no photo was uploaded, and `ready` with an image URL otherwise. Results are cached per outfit pieces and person photo. `placeholder: true` while the image model is stubbed.")]
    [ProducesResponseType<TryOnResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ResponseExample<TryOnReadyExample>(StatusCodes.Status200OK)]
    public async Task<TryOnResult> TryOn(string id, CancellationToken cancellationToken)
    {
        var outfit = await outfits.GetAsync(id, cancellationToken);
        var state = await userStates.GetOrCreateAsync(cancellationToken);
        if (string.IsNullOrEmpty(state.PersonPhotoFile))
            return new TryOnResult { Status = TryOnStatus.NeedsPhoto, Message = "Upload a full-body photo first." };

        var piecesKey = outfit.Pieces.Key();
        var cached = await tryOnRenders.FindAsync(outfit.Id, piecesKey, state.PersonPhotoFile, cancellationToken);
        if (cached is not null) return Ready(cached.ImageFile, cached.Placeholder);

        try
        {
            var closet = await items.ListNewestFirstAsync(cancellationToken);
            var pieces = closet.Where(item => outfit.Pieces.AllIds().Contains(item.Id)).ToList();
            var rendered = await renderer.RenderAsync(state.PersonPhotoFile, pieces, cancellationToken);

            tryOnRenders.Add(new TryOnRender
            {
                Id = EntityId.New(),
                OutfitId = outfit.Id,
                PiecesKey = piecesKey,
                PersonPhotoFile = state.PersonPhotoFile,
                ImageFile = rendered.ImageFile,
                Placeholder = rendered.Placeholder,
                CreatedAt = clock.GetUtcNow(),
            });
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Ready(rendered.ImageFile, rendered.Placeholder);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Try-on rendering failed for outfit {OutfitId}", outfit.Id);
            return new TryOnResult { Status = TryOnStatus.Failed, Message = "The try-on image could not be rendered." };
        }
    }

    private TryOnResult Ready(string imageFile, bool placeholder) => new()
    {
        Status = TryOnStatus.Ready,
        ImageUri = mediaUrls.UrlFor(imageFile),
        Placeholder = placeholder,
    };
}
