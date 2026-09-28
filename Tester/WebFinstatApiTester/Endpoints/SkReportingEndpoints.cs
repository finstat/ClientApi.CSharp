using System.IO;
using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class SkReportingEndpoints
{
    public static IEndpointRouteBuilder MapSkReportingEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sk/reporting");

        group.MapGet("/topics", async (FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.SkReporting(creds);
            var r = await client.RequestTopics(creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        group.MapGet("/list", async (string topic, FinstatCredentials creds) =>
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new BadHttpRequestException("Missing topic query parameter.");
            var client = ApiClientFactory.SkReporting(creds);
            var r = await client.RequestList(topic, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(r, creds.Json));
        });

        group.MapGet("/download", async (string fileName, FinstatCredentials creds) =>
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new BadHttpRequestException("Missing fileName query parameter.");

            var dir = Path.Combine(Path.GetTempPath(), "finstat-web-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var client = ApiClientFactory.SkReporting(creds);
                var path = await client.DownloadReportFile(fileName, dir);
                if (path is null || !File.Exists(path))
                    throw new BadHttpRequestException("FinStat returned no file.");

                var bytes = await File.ReadAllBytesAsync(path);
                return Results.File(bytes, "application/octet-stream", fileName);
            }
            finally
            {
                try { Directory.Delete(dir, recursive: true); } catch { /* best-effort */ }
            }
        });

        return routes;
    }
}
