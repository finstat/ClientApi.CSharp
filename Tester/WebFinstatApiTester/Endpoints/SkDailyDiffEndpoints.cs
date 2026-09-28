using System.IO;
using FinstatApi;
using FinstatApi.ViewModel;
using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class SkDailyDiffEndpoints
{
    /// <summary>XmlSerializer-friendly wrapper for KeyValue[] (legend).</summary>
    public sealed class LegendResult
    {
        public KeyValue[] Items { get; set; } = System.Array.Empty<KeyValue>();
        public LegendResult() { }
        public LegendResult(KeyValue[] items) { Items = items ?? System.Array.Empty<KeyValue>(); }
    }

    public static IEndpointRouteBuilder MapSkDailyDiffEndpoints(this IEndpointRouteBuilder routes)
    {
        var root = routes.MapGroup("/api/sk/diff");

        // ── DAILY ────────────────────────────────────────────────────────────
        root.MapGet("/daily/list", async (FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkDailyDiff(creds);
            var r = await c.RequestListOfDailyDiffs(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });
        root.MapGet("/daily/download", (string fileName, FinstatCredentials creds) =>
            Download(fileName, async dir =>
                await ApiClientFactory.SkDailyDiff(creds).DownloadDailyDiffFile(fileName, dir)));

        // ── ULTIMATE ─────────────────────────────────────────────────────────
        root.MapGet("/ultimate/list", async (FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkDailyUltimateDiff(creds);
            var r = await c.RequestListOfDailyUltimateDiffs(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });
        root.MapGet("/ultimate/download", (string fileName, FinstatCredentials creds) =>
            Download(fileName, async dir =>
                await ApiClientFactory.SkDailyUltimateDiff(creds).DownloadDailyUltimateDiffFile(fileName, dir)));

        // ── STATEMENT ────────────────────────────────────────────────────────
        root.MapGet("/statement/list", async (FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkDailyStatementDiff(creds);
            var r = await c.RequestListOfDailyStatementDiffs(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });
        root.MapGet("/statement/download", (string fileName, FinstatCredentials creds) =>
            Download(fileName, async dir =>
                await ApiClientFactory.SkDailyStatementDiff(creds).DownloadDailyStatementDiffFile(fileName, dir)));
        root.MapGet("/statement/legend", async (string? lang, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkDailyStatementDiff(creds);
            var r = await c.RequestStatementLegend(lang ?? "sk", creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(new LegendResult(r), creds.Json));
        });

        // ── STATEMENT v2014 ──────────────────────────────────────────────────
        root.MapGet("/statement2014/list", async (FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkDailyStatement2014Diff(creds);
            var r = await c.RequestListOfDailyStatement2014Diffs(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });
        root.MapGet("/statement2014/download", (string fileName, FinstatCredentials creds) =>
            Download(fileName, async dir =>
                await ApiClientFactory.SkDailyStatement2014Diff(creds).DownloadDailyStatement2014DiffFile(fileName, dir)));
        root.MapGet("/statement2014/legend", async (string? lang, FinstatCredentials creds) =>
        {
            var c = ApiClientFactory.SkDailyStatement2014Diff(creds);
            var r = await c.RequestStatement2014Legend(lang ?? "sk", creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        return routes;
    }

    /// <summary>
    /// Shared pipeline for the 4 download endpoints. The C# client writes to disk; we use
    /// a per-request temp dir, stream the file to the browser, then delete the dir. FinStat
    /// errors bubble out as <see cref="FinstatApi.FinstatApiException"/> and are handled by
    /// the global <see cref="FinstatExceptionHandler"/>.
    /// </summary>
    private static async Task<IResult> Download(string fileName, System.Func<string, Task<string?>> call)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new BadHttpRequestException("Missing fileName query parameter.");

        var dir = Path.Combine(Path.GetTempPath(), "finstat-web-" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path = await call(dir);
            if (path is null || !File.Exists(path))
                throw new BadHttpRequestException("FinStat returned no file.");

            var bytes = await File.ReadAllBytesAsync(path);
            return Results.File(bytes, "application/octet-stream", fileName);
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { /* best-effort */ }
        }
    }
}
