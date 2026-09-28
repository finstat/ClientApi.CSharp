extern alias CZ;

using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class CzAutoEndpoints
{
    public sealed record AutocompleteRequest(string Query);
    public sealed record AutoLoginRequest(string Url, string? Email);

    public static IEndpointRouteBuilder MapCzAutoEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/cz/autocomplete", async (AutocompleteRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Cz(creds);
            var result = await client.RequestAutocomplete(req.Query, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        routes.MapPost("/api/cz/autologin", async (AutoLoginRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Cz(creds);
            var result = await client.RequestAutoLogin(req.Url, req.Email, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new SkAutoEndpoints.AutoLoginResult(result), creds.Json));
        });

        return routes;
    }
}
