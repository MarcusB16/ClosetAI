using ClosetAPI.Application.Dtos.Items.CreateItem;
using ClosetAPI.Application.Dtos.Items.SuggestItem;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Application.Mapping;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Media;
using ClosetAPI.Domain.Persistence;
using ClosetAPI.Infrastructure.Media;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.OpenApi.Examples;
using Microsoft.AspNetCore.Mvc;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api/items")]
[Tags("Items")]
public class ItemsController(
    IItemRepository items,
    IUnitOfWork unitOfWork,
    TimeProvider clock,
    IMediaStore mediaStore,
    IBackgroundRemover backgroundRemover,
    IItemTagger itemTagger,
    MediaUrlResolver mediaUrls) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("List the closet")]
    [EndpointDescription("Returns every item in the closet, newest first.")]
    [ProducesResponseType<List<ItemResponse>>(StatusCodes.Status200OK)]
    [ResponseExample<ItemListExample>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ItemResponse>>> List(CancellationToken cancellationToken)
    {
        var closet = await items.ListNewestFirstAsync(cancellationToken);
        return closet.Select(item => item.ToResponse()).ToList();
    }

    [HttpPost]
    [EndpointSummary("Add an item")]
    [EndpointDescription("Creates an item from a confirmed suggestion or manual entry. `type` must be a known garment type and `formality` is 1 (casual), 2 (smart) or 3 (formal).")]
    [Consumes("application/json")]
    [ProducesResponseType<ItemResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [RequestExample<NewItemExample>]
    [ResponseExample<ItemExample>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ItemResponse>> Create(NewItem newItem, CancellationToken cancellationToken)
    {
        var item = newItem.ToEntity(EntityId.New(), clock.GetUtcNow());
        items.Add(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item.ToResponse());
    }

    [HttpGet("{id}")]
    [EndpointSummary("Get an item")]
    [ProducesResponseType<ItemResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ResponseExample<ItemExample>(StatusCodes.Status200OK)]
    public async Task<ItemResponse> Get(string id, CancellationToken cancellationToken) =>
        (await items.GetAsync(id, cancellationToken)).ToResponse();

    [HttpPost("analyze")]
    [EndpointSummary("Analyze an item photo")]
    [EndpointDescription("Stores the uploaded photo, removes its background (stubbed: the original is returned as `cutoutUri`) and tags it with AI. Nothing is added to the closet; confirm the suggestion with POST /api/items.")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<ItemSuggestion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway, "application/problem+json")]
    [ResponseExample<ItemSuggestionExample>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ItemSuggestion>> Analyze([FromForm] PhotoUpload upload, CancellationToken cancellationToken)
    {
        var photo = await PhotoUploadReader.ReadAsync(upload.Photo, cancellationToken);
        var photoFile = await mediaStore.SaveAsync(photo, cancellationToken);
        var cutoutFile = await backgroundRemover.RemoveBackgroundAsync(photoFile, cancellationToken);
        var tags = await itemTagger.TagAsync(photo, cancellationToken);

        return new ItemSuggestion
        {
            Name = tags.Name,
            Brand = tags.Brand,
            Category = tags.Category,
            Slot = tags.Slot,
            Type = tags.Type,
            ColorName = tags.ColorName,
            Color = tags.Color,
            Formality = tags.Formality,
            Seasons = tags.Seasons.ToList(),
            Fabric = tags.Fabric,
            CutoutUri = mediaUrls.UrlFor(cutoutFile),
        };
    }
}
