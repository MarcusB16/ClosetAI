using ClosetAPI.Application.Dtos.TryOns.GetTryOn;
using ClosetAPI.Domain.TryOns;
using ClosetAPI.Infrastructure.Media;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.OpenApi.Examples;
using ClosetAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api/outfits/{id}/try-on")]
[Tags("Try-on")]
public class TryOnController(
    ClosetDbContext db,
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
        var outfit = await db.GetOutfitAsync(id, cancellationToken);
        var state = await db.GetUserStateAsync(cancellationToken);
        if (string.IsNullOrEmpty(state.PersonPhotoFile))
            return new TryOnResult { Status = TryOnStatus.NeedsPhoto, Message = "Upload a full-body photo first." };

        var piecesKey = outfit.Pieces.Key();
        var cached = await db.TryOnRenders.FirstOrDefaultAsync(
            render => render.OutfitId == outfit.Id && render.PiecesKey == piecesKey && render.PersonPhotoFile == state.PersonPhotoFile,
            cancellationToken);
        if (cached is not null) return Ready(cached.ImageFile, cached.Placeholder);

        try
        {
            var closet = await db.LoadClosetAsync(cancellationToken);
            var pieces = closet.Where(item => outfit.Pieces.AllIds().Contains(item.Id)).ToList();
            var rendered = await renderer.RenderAsync(state.PersonPhotoFile, pieces, cancellationToken);

            db.TryOnRenders.Add(new TryOnRender
            {
                Id = ClosetDbContextExtensions.NewId(),
                OutfitId = outfit.Id,
                PiecesKey = piecesKey,
                PersonPhotoFile = state.PersonPhotoFile,
                ImageFile = rendered.ImageFile,
                Placeholder = rendered.Placeholder,
                CreatedAt = clock.GetUtcNow(),
            });
            await db.SaveChangesAsync(cancellationToken);
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
