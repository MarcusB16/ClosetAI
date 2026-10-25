namespace ClosetAPI.Infrastructure.Media;

public class MediaUrlResolver(IHttpContextAccessor httpContextAccessor)
{
    public string UrlFor(string fileName)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        var path = $"{MediaOptions.RequestPath}/{fileName}";
        return request is null ? path : $"{request.Scheme}://{request.Host}{request.PathBase}{path}";
    }

    public string? UrlForOrNull(string? fileName) => string.IsNullOrEmpty(fileName) ? null : UrlFor(fileName);
}
