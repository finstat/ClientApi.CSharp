using FinstatApi.Statement;
using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class SkStatementEndpoints
{
    public sealed record StatementDetailBody(string Ico, int Year, string Template);

    public static IEndpointRouteBuilder MapSkStatementEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sk/statement");

        group.MapGet("/list", async (string ico, FinstatCredentials creds) =>
        {
            if (string.IsNullOrWhiteSpace(ico))
                throw new BadHttpRequestException("Missing ico query parameter.");
            var client = ApiClientFactory.SkStatement(creds);
            var r = await client.RequestStatements(ico, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        group.MapPost("/detail", async (StatementDetailBody req, FinstatCredentials creds) =>
        {
            if (!Enum.TryParse<TemplateTypeEnum>(req.Template, ignoreCase: true, out var tpl))
                throw new BadHttpRequestException(
                    $"Template must be one of: {string.Join(", ", Enum.GetNames<TemplateTypeEnum>())}.");

            var client = ApiClientFactory.SkStatement(creds);
            var r = await client.RequestStatementDetail(req.Ico, req.Year, tpl, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        group.MapGet("/legend", async (string template, string? lang, FinstatCredentials creds) =>
        {
            if (!Enum.TryParse<TemplateTypeEnum>(template, ignoreCase: true, out var tpl))
                throw new BadHttpRequestException(
                    $"Template must be one of: {string.Join(", ", Enum.GetNames<TemplateTypeEnum>())}.");

            var client = ApiClientFactory.SkStatement(creds);
            var r = await client.RequestStatementLegend(tpl, lang ?? "sk", creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        return routes;
    }
}
