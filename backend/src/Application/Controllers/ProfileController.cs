using ClosetAPI.Application.Dtos.Profiles.UpdateProfile;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Application.Mapping;
using ClosetAPI.Domain.Users;
using ClosetAPI.Domain.Media;
using ClosetAPI.Infrastructure.Media;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.OpenApi.Examples;
using ClosetAPI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api/profile")]
[Tags("Profile")]
public class ProfileController(ClosetDbContext db, IMediaStore mediaStore, MediaUrlResolver mediaUrls) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get the profile")]
    [EndpointDescription("Returns onboarding state and the person photo URL, if one was uploaded.")]
    [ProducesResponseType<Profile>(StatusCodes.Status200OK)]
    [ResponseExample<ProfileExample>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Profile>> Get(CancellationToken cancellationToken) =>
        ToProfile(await db.GetUserStateAsync(cancellationToken));

    [HttpPatch]
    [EndpointSummary("Update the profile")]
    [EndpointDescription("Partial update. Send { \"onboarded\": true } to finish first-launch setup.")]
    [Consumes("application/json")]
    [ProducesResponseType<Profile>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [RequestExample<UpdateProfileRequestExample>]
    [ResponseExample<ProfileExample>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Profile>> Update(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var state = await db.GetUserStateAsync(cancellationToken);
        if (request.Onboarded is { } onboarded) state.Onboarded = onboarded;
        await db.SaveChangesAsync(cancellationToken);
        return ToProfile(state);
    }

    [HttpPut("person-photo")]
    [EndpointSummary("Upload the person photo")]
    [EndpointDescription("Multipart upload (field `photo`, JPEG or PNG, max 10 MB) of a full-body photo used for try-on.")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType<Profile>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ResponseExample<ProfileExample>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Profile>> UploadPersonPhoto([FromForm] PhotoUpload upload, CancellationToken cancellationToken)
    {
        var photo = await PhotoUploadReader.ReadAsync(upload.Photo, cancellationToken);
        var state = await db.GetUserStateAsync(cancellationToken);
        state.PersonPhotoFile = await mediaStore.SaveAsync(photo, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToProfile(state);
    }

    [HttpDelete("person-photo")]
    [EndpointSummary("Remove the person photo")]
    [EndpointDescription("Clears the saved person photo. Try-on will report `needsPhoto` until a new one is uploaded.")]
    [ProducesResponseType<Profile>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Profile>> DeletePersonPhoto(CancellationToken cancellationToken)
    {
        var state = await db.GetUserStateAsync(cancellationToken);
        state.PersonPhotoFile = null;
        await db.SaveChangesAsync(cancellationToken);
        return ToProfile(state);
    }

    private Profile ToProfile(UserState state) => new()
    {
        Onboarded = state.Onboarded,
        PersonPhotoUri = mediaUrls.UrlForOrNull(state.PersonPhotoFile),
    };
}
