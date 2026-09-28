using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class SkAutoEndpoints
{
    public sealed record AutocompleteRequest(string Query);
    public sealed record AutoLoginRequest(string Url, string? Email);

    public static IEndpointRouteBuilder MapSkAutoEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/sk/autocomplete", async (AutocompleteRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Sk(creds);
            var result = await client.RequestAutocomplete(req.Query, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        routes.MapPost("/api/sk/autologin", async (AutoLoginRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Sk(creds);
            var result = await client.RequestAutoLogin(req.Url, req.Email, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new AutoLoginResult(result), creds.Json));
        });

        return routes;
    }

    /// <summary>
    /// Wrap the bare string response so the envelope can XML-serialise it.
    /// (XmlSerializer needs a public parameterless ctor — records don't have one.)
    /// </summary>
    public sealed class AutoLoginResult
    {
        public string? Url { get; set; }

        public AutoLoginResult() { }
        public AutoLoginResult(string? url) { Url = url; }
    }
}
