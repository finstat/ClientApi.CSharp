extern alias CZ;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Translates FinStat SK/CZ API exceptions into RFC-7807 ProblemDetails responses.
/// Also emits a single log line per handled exception so ops can see failures in
/// <c>docker logs</c> without scraping the response body.
/// </summary>
public sealed class FinstatExceptionHandler(ILogger<FinstatExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            Birko.Validation.ValidationException ve => BuildValidation(ve),
            FinstatApi.FinstatApiException sk    => BuildSk(sk),
            CZ::FinstatApi.FinstatApiException cz => BuildCz(cz),
            BadHttpRequestException bad          => BuildBadRequest(bad),
            _ => null,
        };
        if (problem is null)
        {
            // Unknown exceptions propagate to ASP.NET's default handler which logs at Error level.
            logger.LogError(exception, "Unhandled exception on {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
            return false;
        }

        logger.LogWarning(
            "{Title} on {Method} {Path} -> {Status}: {Detail}",
            problem.Title, httpContext.Request.Method, httpContext.Request.Path,
            problem.Status ?? 500, problem.Detail);

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";
        // Serialize via the runtime type so derived payloads (e.g. ValidationProblemDetails.Errors)
        // survive — `WriteAsJsonAsync<ProblemDetails>` would strip subclass properties.
        await httpContext.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, cancellationToken);
        return true;
    }

    private static ProblemDetails BuildSk(FinstatApi.FinstatApiException ex) => new()
    {
        Title    = "FinStat SK API error",
        Detail   = ex.Message,
        Status   = MapStatus(ex.FailType),
        Type     = "https://finstat.github.io/api-documentation/",
        Extensions = { ["failType"] = ex.FailType.ToString() },
    };

    private static ProblemDetails BuildCz(CZ::FinstatApi.FinstatApiException ex) => new()
    {
        Title    = "FinStat CZ API error",
        Detail   = ex.Message,
        Status   = MapStatusCz(ex.FailType),
        Type     = "https://finstat.github.io/api-documentation/",
        Extensions = { ["failType"] = ex.FailType.ToString() },
    };

    private static ProblemDetails BuildBadRequest(BadHttpRequestException ex) => new()
    {
        Title  = "Bad request",
        Detail = ex.Message,
        Status = StatusCodes.Status400BadRequest,
    };

    private static ValidationProblemDetails BuildValidation(Birko.Validation.ValidationException ex) =>
        new(ex.ValidationResult.ToDictionary())
        {
            Title  = "Invalid settings",
            Status = StatusCodes.Status400BadRequest,
        };

    private static int MapStatus(FinstatApi.FinstatApiException.FailTypeEnum ft) => ft switch
    {
        FinstatApi.FinstatApiException.FailTypeEnum.NotFound             => StatusCodes.Status404NotFound,
        FinstatApi.FinstatApiException.FailTypeEnum.NotValidCustomerKey  => StatusCodes.Status401Unauthorized,
        FinstatApi.FinstatApiException.FailTypeEnum.InvalidHash          => StatusCodes.Status401Unauthorized,
        FinstatApi.FinstatApiException.FailTypeEnum.Unauthorized         => StatusCodes.Status401Unauthorized,
        FinstatApi.FinstatApiException.FailTypeEnum.LicenseExpired       => StatusCodes.Status403Forbidden,
        FinstatApi.FinstatApiException.FailTypeEnum.InsufficientAccess   => StatusCodes.Status403Forbidden,
        FinstatApi.FinstatApiException.FailTypeEnum.AccessDisabled       => StatusCodes.Status403Forbidden,
        FinstatApi.FinstatApiException.FailTypeEnum.LimitExceed          => StatusCodes.Status429TooManyRequests,
        FinstatApi.FinstatApiException.FailTypeEnum.Timeout              => StatusCodes.Status504GatewayTimeout,
        FinstatApi.FinstatApiException.FailTypeEnum.BadRequest           => StatusCodes.Status400BadRequest,
        FinstatApi.FinstatApiException.FailTypeEnum.TooShort             => StatusCodes.Status400BadRequest,
        FinstatApi.FinstatApiException.FailTypeEnum.GdprRestriction      => StatusCodes.Status451UnavailableForLegalReasons,
        FinstatApi.FinstatApiException.FailTypeEnum.UrlNotFound          => StatusCodes.Status502BadGateway,
        FinstatApi.FinstatApiException.FailTypeEnum.OtherCommunicationFail => StatusCodes.Status502BadGateway,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static int MapStatusCz(CZ::FinstatApi.FinstatApiException.FailTypeEnum ft) => ft switch
    {
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.NotFound             => StatusCodes.Status404NotFound,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.NotValidCustomerKey  => StatusCodes.Status401Unauthorized,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.InvalidHash          => StatusCodes.Status401Unauthorized,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.Unauthorized         => StatusCodes.Status401Unauthorized,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.LicenseExpired       => StatusCodes.Status403Forbidden,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.InsufficientAccess   => StatusCodes.Status403Forbidden,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.AccessDisabled       => StatusCodes.Status403Forbidden,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.LimitExceed          => StatusCodes.Status429TooManyRequests,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.Timeout              => StatusCodes.Status504GatewayTimeout,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.BadRequest           => StatusCodes.Status400BadRequest,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.TooShort             => StatusCodes.Status400BadRequest,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.GdprRestriction      => StatusCodes.Status451UnavailableForLegalReasons,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.UrlNotFound          => StatusCodes.Status502BadGateway,
        CZ::FinstatApi.FinstatApiException.FailTypeEnum.OtherCommunicationFail => StatusCodes.Status502BadGateway,
        _ => StatusCodes.Status500InternalServerError,
    };
}
