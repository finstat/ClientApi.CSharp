using FinstatApi;
using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class SkMonitoringEndpoints
{
    public sealed record IcoBody(string Ico, string? Category);
    public sealed record DateBody(string Date, string? Category);

    /// <summary>Wrap bool / string[] results so XmlSerializer has a concrete root element.</summary>
    public sealed class OkResult
    {
        public bool Value { get; set; }
        public OkResult() { }
        public OkResult(bool v) { Value = v; }
    }
    public sealed class StringsResult
    {
        public string[] Values { get; set; } = System.Array.Empty<string>();
        public StringsResult() { }
        public StringsResult(string[] v) { Values = v ?? System.Array.Empty<string>(); }
    }

    public static IEndpointRouteBuilder MapSkMonitoringEndpoints(this IEndpointRouteBuilder routes)
    {
        var root = routes.MapGroup("/api/sk/monitoring");

        // ── Categories ────────────────────────────────────────────────────────
        root.MapGet("/categories", async (FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = await c.GetCategories(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        // ── ICO list/add/remove/report/proceedings ────────────────────────────
        root.MapGet("/list", async (string? category, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(category)
                ? await c.GetMonitorings(creds.Json)
                : await c.GetMonitorings(category, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new StringsResult(r), creds.Json));
        });

        root.MapPost("/add", async (IcoBody req, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(req.Category)
                ? await c.Add(req.Ico, creds.Json)
                : await c.Add(req.Ico, req.Category!, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new OkResult(r), creds.Json));
        });

        root.MapPost("/remove", async (IcoBody req, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(req.Category)
                ? await c.Remove(req.Ico, creds.Json)
                : await c.Remove(req.Ico, req.Category!, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new OkResult(r), creds.Json));
        });

        root.MapGet("/report", async (string? category, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(category)
                ? await c.GetReport(creds.Json)
                : await c.GetReport(category, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        root.MapGet("/proceedings", async (FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = await c.GetProceedings(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        // ── Date list/add/remove/report/proceedings ───────────────────────────
        var date = root.MapGroup("/date");

        date.MapGet("/list", async (string? category, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(category)
                ? await c.GetDateMonitorings(creds.Json)
                : await c.GetDateMonitorings(category, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new StringsResult(r), creds.Json));
        });

        date.MapPost("/add", async (DateBody req, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(req.Category)
                ? await c.AddDate(req.Date, creds.Json)
                : await c.AddDate(req.Date, req.Category!, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new OkResult(r), creds.Json));
        });

        date.MapPost("/remove", async (DateBody req, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(req.Category)
                ? await c.RemoveDate(req.Date, creds.Json)
                : await c.RemoveDate(req.Date, req.Category!, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new OkResult(r), creds.Json));
        });

        date.MapGet("/report", async (string? category, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = string.IsNullOrWhiteSpace(category)
                ? await c.GetDateReport(creds.Json)
                : await c.GetDateReport(category, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        date.MapGet("/proceedings", async (FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkMonitoring(creds);
            var r = await c.GetDateProceedings(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        return routes;
    }
}
