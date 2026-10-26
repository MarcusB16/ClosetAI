using ClosetAPI.Domain.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ClosetAPI.Infrastructure.Http;

public class ApiExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            EntityNotFoundException => new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Not found", Detail = exception.Message },
            RuleViolationException violation => ToValidationProblem(violation),
            AiProviderException => new ProblemDetails { Status = StatusCodes.Status502BadGateway, Title = "AI provider failure", Detail = exception.Message },
            _ => null,
        };
        if (problem is null) return false;

        if (problem.Status == StatusCodes.Status502BadGateway)
            logger.LogError(exception, "AI provider call failed");

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problem, Exception = exception });
    }

    private static ValidationProblemDetails ToValidationProblem(RuleViolationException violation) => new(
        new Dictionary<string, string[]> { [violation.Field] = [violation.Message] })
    {
        Status = StatusCodes.Status400BadRequest,
        Title = "One or more validation errors occurred.",
    };
}
