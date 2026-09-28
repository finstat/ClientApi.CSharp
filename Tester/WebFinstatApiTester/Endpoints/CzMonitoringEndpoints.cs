extern alias CZ;

using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class CzMonitoringEndpoints
{
    public sealed record IcoBody(string Ico, string? Category);
    public sealed record DateBody(string Date, string? Category);

    public static IEndpointRouteBuilder MapCzMonitoringEndpoints(this IEndpointRouteBuilder routes)
    {
        var root = routes.MapGroup("/api/cz/monitoring");

        root.MapGet("/categories", async (FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = await c.GetCategories(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        root.MapGet("/list", async (string? category, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(category)
                ? await c.GetMonitorings(creds.Json)
                : await c.GetMonitorings(category, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new SkMonitoringEndpoints.StringsResult(r), creds.Json));
        });

        root.MapPost("/add", async (IcoBody req, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(req.Category)
                ? await c.Add(req.Ico, creds.Json)
                : await c.Add(req.Ico, req.Category!, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new SkMonitoringEndpoints.OkResult(r), creds.Json));
        });

        root.MapPost("/remove", async (IcoBody req, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(req.Category)
                ? await c.Remove(req.Ico, creds.Json)
                : await c.Remove(req.Ico, req.Category!, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new SkMonitoringEndpoints.OkResult(r), creds.Json));
        });

        root.MapGet("/report", async (string? category, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(category)
                ? await c.GetReport(creds.Json)
                : await c.GetReport(category, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        // ── Date list/add/remove/report (CZ has no Proceedings variant) ───────
        var date = root.MapGroup("/date");

        date.MapGet("/list", async (string? category, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(category)
                ? await c.GetDateMonitorings(creds.Json)
                : await c.GetDateMonitorings(category, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new SkMonitoringEndpoints.StringsResult(r), creds.Json));
        });

        date.MapPost("/add", async (DateBody req, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(req.Category)
                ? await c.AddDate(req.Date, creds.Json)
                : await c.AddDate(req.Date, req.Category!, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new SkMonitoringEndpoints.OkResult(r), creds.Json));
        });

        date.MapPost("/remove", async (DateBody req, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(req.Category)
                ? await c.RemoveDate(req.Date, creds.Json)
                : await c.RemoveDate(req.Date, req.Category!, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new SkMonitoringEndpoints.OkResult(r), creds.Json));
        });

        date.MapGet("/report", async (string? category, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.CzMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(category)
                ? await c.GetDateReport(creds.Json)
                : await c.GetDateReport(category, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        return routes;
    }
}
