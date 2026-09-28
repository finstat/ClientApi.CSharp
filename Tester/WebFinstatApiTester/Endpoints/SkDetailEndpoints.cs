using FinstatApi;
using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class SkDetailEndpoints
{
    public sealed record IcoRequest(string Ico);

    public static IEndpointRouteBuilder MapSkDetailEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sk/detail");

        group.MapPost("/basic", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Sk(creds);
            var result = await client.RequestBasic(req.Ico, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        group.MapPost("/detail", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Sk(creds);
            var result = await client.RequestDetail(req.Ico, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        group.MapPost("/extended", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Sk(creds);
            var result = await client.RequestExtendedDetail(req.Ico, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        group.MapPost("/ultimate", async (IcoRequest req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.Sk(creds);
            var result = await client.RequestUltimateDetail(req.Ico, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(result, creds.Json));
        });

        return routes;
    }
}
