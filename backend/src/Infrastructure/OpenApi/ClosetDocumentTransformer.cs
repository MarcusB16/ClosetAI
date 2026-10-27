using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ClosetAPI.Infrastructure.OpenApi;

public class ClosetDocumentTransformer : IOpenApiDocumentTransformer
{
    private static readonly (string Name, string Description)[] Tags =
    [
        ("Context", "Today's date and weather used for styling."),
        ("Profile", "Onboarding state and the full-body photo used for try-on."),
        ("Items", "The wardrobe: list, add, and analyze photos of new pieces."),
        ("Outfits", "Generate, refine, swap, reject and save outfits built from the closet."),
        ("Wears", "Log what was worn and read the wear history."),
        ("Try-on", "Render an outfit on the profile photo (stubbed in v1)."),
        ("Demo", "Development-only helpers."),
    ];

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "ClosetAI API",
            Version = "v1",
            Description = "Single-user wardrobe and AI stylist API. Errors are RFC 9457 problem+json.",
        };
        document.Tags = new HashSet<OpenApiTag>(Tags.Select(tag => new OpenApiTag { Name = tag.Name, Description = tag.Description }));
        return Task.CompletedTask;
    }
}
