using ClosetAPI.Domain.Context;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.OpenApi.Examples;
using Microsoft.AspNetCore.Mvc;
using StyleContextResponse = ClosetAPI.Application.Dtos.Shared.StyleContext;

namespace ClosetAPI.Application.Controllers;

[ApiController]
[Route("api/context")]
[Tags("Context")]
public class ContextController(IStyleContextProvider contextProvider) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get today's style context")]
    [EndpointDescription("Returns today's date, weekday and weather. Weather is a seeded value in v1.")]
    [ProducesResponseType<StyleContextResponse>(StatusCodes.Status200OK)]
    [ResponseExample<StyleContextExample>(StatusCodes.Status200OK)]
    public ActionResult<StyleContextResponse> Get()
    {
        var context = contextProvider.Current();
        return new StyleContextResponse
        {
            Date = context.Date,
            Weekday = context.Weekday,
            TempF = context.TempF,
            Forecast = context.Forecast,
            Rain = context.Rain,
        };
    }
}
