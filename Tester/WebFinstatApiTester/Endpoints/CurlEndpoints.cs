using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class CurlEndpoints
{
    public static IEndpointRouteBuilder MapCurlEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/_curl", (CurlBuilder.Request req, FinstatCredentials creds) =>
        {
            creds.EnsureValid();
            return Results.Ok(CurlBuilder.Build(req, creds));
        });

        return routes;
    }
}
