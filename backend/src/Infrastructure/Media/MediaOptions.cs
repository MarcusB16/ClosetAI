namespace ClosetAPI.Infrastructure.Media;

public class MediaOptions
{
    public const string SectionName = "Media";
    public const string RequestPath = "/media";

    public string Folder { get; set; } = "media";
}
