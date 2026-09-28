using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class SkDistraintEndpoints
{
    /// <summary>Parameters shared by Search / Results — all optional, the caller supplies the combination they have.</summary>
    public sealed record DistraintQuery(
        string? Ico,
        string? Surname,
        string? DateOfBirth,
        string? City,
        string? CompanyName,
        string? FileReference);

    public sealed record DistraintDetailBody(string Token, int[] Ids);
    public sealed record DistraintTokenBody(string Token);
    public sealed record DistraintStoredBody(string Id);

    public static IEndpointRouteBuilder MapSkDistraintEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sk/distraint");

        group.MapPost("/search", async (DistraintQuery req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.SkDistraint(creds);
            var r = await client.RequestDistraintSearch(
                req.Ico ?? "", req.Surname ?? "", req.DateOfBirth ?? "",
                req.City ?? "", req.CompanyName ?? "", req.FileReference ?? "",
                creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        group.MapPost("/results", async (DistraintQuery req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.SkDistraint(creds);
            var r = await client.RequestDistraintResults(
                req.Ico ?? "", req.Surname ?? "", req.DateOfBirth ?? "",
                req.City ?? "", req.CompanyName ?? "", req.FileReference ?? "",
                creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        group.MapPost("/detail", async (DistraintDetailBody req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.SkDistraint(creds);
            var r = await client.RequestDistraintDetail(req.Token, req.Ids ?? System.Array.Empty<int>(), creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        group.MapPost("/results-by-token", async (DistraintTokenBody req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.SkDistraint(creds);
            var r = await client.RequestDistraintResultsByToken(req.Token, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        group.MapPost("/stored-detail", async (DistraintStoredBody req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.SkDistraint(creds);
            var r = await client.RequestDistraintStoredDetail(req.Id, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        return routes;
    }
}
