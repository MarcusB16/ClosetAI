using ClosetAPI.Application.Dtos.Outfits.SetSaved;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class SetSavedRequestExample : IOpenApiExample
{
    public static object Value => new SetSavedRequest { Saved = true };
}
