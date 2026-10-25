using ClosetAPI.Application.Dtos.Outfits.RefineOutfit;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public class RefineRequestExample : IOpenApiExample
{
    public static object Value => new RefineRequest { Instruction = "different shoes" };
}
