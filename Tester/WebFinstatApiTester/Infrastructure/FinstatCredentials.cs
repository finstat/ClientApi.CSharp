namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Per-request credentials + settings, bound from X-Finstat-* request headers so the
/// browser can keep them in localStorage and send them with each call. Minimal APIs
/// discover <see cref="BindAsync"/> automatically — just declare the parameter.
/// </summary>
public sealed class FinstatCredentials
{
    public const string DefaultUrlSk = "https://www.finstat.sk/api";
    public const string DefaultUrlCz = "https://cz.finstat.sk/api";
    public const string DefaultStationId = "api-tester";
    public const string DefaultStationName = "Api Tester Web";
    public const int DefaultTimeOut = 30_000;

    public string ApiKey { get; init; } = string.Empty;
    public string PrivateKey { get; init; } = string.Empty;
    public string StationID { get; init; } = DefaultStationId;
    public string StationName { get; init; } = DefaultStationName;
    public int TimeOut { get; init; } = DefaultTimeOut;
    public string UrlSk { get; init; } = DefaultUrlSk;
    public string UrlCz { get; init; } = DefaultUrlCz;

    /// <summary>True when the browser requested JSON responses (else XML).</summary>
    public bool Json { get; init; }

    private static readonly FinstatCredentialsValidator Validator = new();

    public void EnsureValid()
    {
        var result = Validator.Validate(this);
        if (!result.IsValid) throw new Birko.Validation.ValidationException(result);
    }

    public static ValueTask<FinstatCredentials?> BindAsync(HttpContext ctx)
    {
        var h = ctx.Request.Headers;
        var creds = new FinstatCredentials
        {
            ApiKey      = h["X-Finstat-ApiKey"].FirstOrDefault() ?? string.Empty,
            PrivateKey  = h["X-Finstat-PrivateKey"].FirstOrDefault() ?? string.Empty,
            StationID   = FirstNonEmpty(h["X-Finstat-StationID"].FirstOrDefault(), DefaultStationId),
            StationName = FirstNonEmpty(h["X-Finstat-StationName"].FirstOrDefault(), DefaultStationName),
            TimeOut     = int.TryParse(h["X-Finstat-TimeOut"].FirstOrDefault(), out var t) && t > 0 ? t : DefaultTimeOut,
            UrlSk       = FirstNonEmpty(h["X-Finstat-Url"].FirstOrDefault(), DefaultUrlSk),
            UrlCz       = FirstNonEmpty(h["X-Finstat-UrlCZ"].FirstOrDefault(), DefaultUrlCz),
            Json        = string.Equals(h["X-Finstat-ResponseType"].FirstOrDefault(), "json",
                                        StringComparison.OrdinalIgnoreCase),
        };
        return ValueTask.FromResult<FinstatCredentials?>(creds);
    }

    private static string FirstNonEmpty(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
