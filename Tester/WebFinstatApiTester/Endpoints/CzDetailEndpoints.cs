extern alias CZ;

using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class CzDetailEndpoints
{
    public sealed record IcoRequest(string Ico);

    public static IEndpointRouteBuilder MapCzDetailEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/cz/detail");

        group.MapPost("/basic", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Cz(creds);
            var result = await client.RequestBasic(req.Ico, creds.Json, creds.Lang);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        group.MapPost("/detail", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Cz(creds);
            var result = await client.RequestDetail(req.Ico, creds.Json, creds.Lang);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        group.MapPost("/premium", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Cz(creds);
            var result = await client.RequestPremium(req.Ico, creds.Json, creds.Lang);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        group.MapPost("/elite", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Cz(creds);
            var result = await client.RequestElite(req.Ico, creds.Json, creds.Lang);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        group.MapPost("/ultimate", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Cz(creds);
            var result = await client.RequestUltimate(req.Ico, creds.Json, creds.Lang);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        return routes;
    }
}
