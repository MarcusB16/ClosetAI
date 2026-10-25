using ClosetAPI.Infrastructure.Seed;
using Microsoft.AspNetCore.Mvc;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api/demo")]
[Tags("Demo")]
public class DemoController(DemoDataSeeder seeder, IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("reset")]
    [EndpointSummary("Reset demo data")]
    [EndpointDescription("Wipes all data and reloads the seeded demo closet. Available in Development only; returns 404 elsewhere.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Reset(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment()) return Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found");

        await seeder.ResetAsync(cancellationToken);
        return NoContent();
    }
}
