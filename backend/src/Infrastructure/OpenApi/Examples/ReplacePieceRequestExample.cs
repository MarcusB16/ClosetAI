using ClosetAPI.Application.Dtos.Outfits.ReplacePiece;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class ReplacePieceRequestExample : IOpenApiExample
{
    public static object Value => new ReplacePieceRequest { ItemId = "s4" };
}
