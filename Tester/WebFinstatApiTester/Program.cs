using Birko.Health;
using Birko.Health.Checks;
using Birko.Security.Vault.Configuration;
using WebFinstatApiTester.Endpoints;
using WebFinstatApiTester.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// LocalVault — when LOCAL_VAULT_ENABLED=true, overlays IConfiguration with KV secrets from a
// running HashiCorp Vault. Keys with `--` inside them map onto nested config paths, so a
// vault field Security--DevCertificate--Fingerprint binds straight into
// DevCertificateSettings.Fingerprint alongside appsettings.*.json values.
Console.WriteLine(
    $"LocalVault: ENABLED={Environment.GetEnvironmentVariable("LOCAL_VAULT_ENABLED") ?? "<unset>"}, " +
    $"ADDR={Environment.GetEnvironmentVariable("LOCAL_VAULT_ADDR") ?? "<unset>"}, " +
    $"DOMAIN={Environment.GetEnvironmentVariable("LOCAL_VAULT_DOMAIN") ?? "<unset>"}, " +
    $"ENVIRONMENT={Environment.GetEnvironmentVariable("LOCAL_VAULT_ENVIRONMENT") ?? "<unset>"}, " +
    $"TOKEN={(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOCAL_VAULT_TOKEN")) ? "<unset>" : "<set>")}");
builder.Configuration.AddLocalVaultConfiguration("finstat-apitester");

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<FinstatExceptionHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// /health — disk + memory tagged "ready" (K8s readiness), FinStat outbound probes tagged
// "live" (liveness). Filter with ?tag=ready or ?tag=live.
builder.Services.AddSingleton(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>();
    return new HealthCheckRunner(defaultTimeout: TimeSpan.FromSeconds(5))
        .Register("disk",       new DiskSpaceHealthCheck(AppContext.BaseDirectory), "ready")
        .Register("memory",     new MemoryHealthCheck(), "ready")
        .Register("finstat-sk", new FinstatReachabilityHealthCheck(http, FinstatCredentials.DefaultUrlSk), "live")
        .Register("finstat-cz", new FinstatReachabilityHealthCheck(http, FinstatCredentials.DefaultUrlCz), "live");
});

var app = builder.Build();

FinstatLimitsHeaders.UseHttpContext(app.Services.GetRequiredService<IHttpContextAccessor>());
FinstatEnvelopeBuilder.UseHttpContext(app.Services.GetRequiredService<IHttpContextAccessor>());

// Dev-only client certificate config — injects the root_ca_dev_zdrojak.eu cert when the
// outbound URL targets a dev endpoint. Matches DesktopFinstatApiTester behaviour.
// Fingerprint resolution:
//   1. Security:DevCertificate:Fingerprint in IConfiguration — populated from either
//      appsettings.*.json (local override) or LocalVault (secret `Security--DevCertificate--Fingerprint`).
//   2. Issuer-name fallback in DevCertificateResolver (root_ca_dev_zdrojak.eu).
DevCertificateResolver.UseSettings(
    app.Configuration.GetSection(DevCertificateSettings.SectionName).Get<DevCertificateSettings>()
    ?? new DevCertificateSettings());

app.UseExceptionHandler();
app.UseStatusCodePages();

// Inbound request log — one stdout line per /api/* hit. `docker logs` picks it up.
// Scoped to /api/ so static assets don't spam. Runs before static files so 404s on
// unknown /api/* routes are also captured.
app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Path.StartsWithSegments("/api"))
    {
        await next(ctx);
        return;
    }
    var sw = System.Diagnostics.Stopwatch.StartNew();
    try
    {
        await next(ctx);
        Console.WriteLine(
            $"Http: {ctx.Request.Method} {ctx.Request.Path}{ctx.Request.QueryString} " +
            $"-> {ctx.Response.StatusCode} in {sw.ElapsedMilliseconds}ms");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"Http: {ctx.Request.Method} {ctx.Request.Path}{ctx.Request.QueryString} " +
            $"FAILED in {sw.ElapsedMilliseconds}ms — {ex.GetType().Name}: {ex.Message}");
        throw;
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", async (HealthCheckRunner runner, string? tag, CancellationToken ct) =>
{
    var report = await runner.RunAsync(tag, ct);
    var status = report.Status == HealthStatus.Unhealthy
        ? StatusCodes.Status503ServiceUnavailable
        : StatusCodes.Status200OK;
    return Results.Json(new
    {
        status     = report.Status.ToString(),
        durationMs = report.TotalDuration.TotalMilliseconds,
        entries    = report.Entries.ToDictionary(
            e => e.Key,
            e => new
            {
                status      = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs  = e.Value.Duration.TotalMilliseconds,
                data        = e.Value.Data,
            }),
    }, statusCode: status);
});

// Probe — verifies X-Finstat-* header binding without leaking secrets.
app.MapGet("/api/whoami", (FinstatCredentials creds) => Results.Ok(new
{
    hasApiKey     = !string.IsNullOrWhiteSpace(creds.ApiKey),
    hasPrivateKey = !string.IsNullOrWhiteSpace(creds.PrivateKey),
    creds.StationID,
    creds.StationName,
    creds.TimeOut,
    creds.UrlSk,
    creds.UrlCz,
    creds.Json,
}));

// Pre-flight validation for the settings modal. Candidate settings arrive as X-Finstat-*
// headers (same shape as every other call), so the binding is identical — just call
// EnsureValid() and let FinstatExceptionHandler map ValidationException → ValidationProblemDetails.
app.MapPost("/api/settings/validate", (FinstatCredentials creds) =>
{
    creds.EnsureValid();
    return Results.NoContent();
});

app.MapSkDetailEndpoints();
app.MapSkAutoEndpoints();
app.MapSkMonitoringEndpoints();
app.MapSkDistraintEndpoints();
app.MapSkDailyDiffEndpoints();
app.MapSkBankruptcyEndpoints();
app.MapSkStatementEndpoints();
app.MapSkReportingEndpoints();
app.MapCzDetailEndpoints();
app.MapCzAutoEndpoints();
app.MapCzMonitoringEndpoints();
app.MapCurlEndpoints();
app.MapAboutEndpoints();

app.MapFallbackToFile("index.html");

app.Run();
