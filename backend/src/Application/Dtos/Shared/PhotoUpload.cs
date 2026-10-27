namespace ClosetAPI.Application.Dtos.Shared;

public record PhotoUpload
{
    public required IFormFile Photo { get; init; }
}
