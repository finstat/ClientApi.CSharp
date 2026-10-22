extern alias CZ;

using System.Text.Json;
using FinstatApi;

namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Lifts per-request FinStat client state onto HTTP response headers so the browser
/// can mirror the desktop tester's UI:
/// <list type="bullet">
///   <item>Quota counters from <c>OnResponse</c> → status-bar "Daily X/Y · Monthly X/Y".</item>
///   <item>The actual outbound form-POST params from <c>OnRequest</c> (apiKey, StationId,
///         StationName, ico, Hash, …) → Request tab in the response-history viewer.</item>
///   <item>The response body exactly as FinStat sent it, from <c>OnResponseContent</c> →
///         kept on the request for <see cref="FinstatEnvelopeBuilder"/> (the Raw tab).</item>
/// </list>
/// Both hook fire before our response is written, so headers are still mutable.
/// Works uniformly for JSON envelopes and binary downloads; zero touches at call sites.
/// </summary>
internal static class FinstatLimitsHeaders
{
    public const string DailyCurrent   = "X-Finstat-Daily-Current";
    public const string DailyMax       = "X-Finstat-Daily-Max";
    public const string MonthlyCurrent = "X-Finstat-Monthly-Current";
    public const string MonthlyMax     = "X-Finstat-Monthly-Max";
    public const string RequestParams  = "X-Finstat-Request-Params";

    private static IHttpContextAccessor? s_accessor;

    public static void UseHttpContext(IHttpContextAccessor accessor) => s_accessor = accessor;

    public static void Attach(FinstatApi.CommonAbstractApiClient client)
    {
        var accessor = s_accessor;
        if (accessor is null) return;
        client.OnResponse += _ =>
        {
            var l = client.Limits;
            if (l is null) return;
            WriteLimits(accessor.HttpContext?.Response,
                l.Daily?.Current ?? 0, l.Daily?.Max ?? 0,
                l.Monthly?.Current ?? 0, l.Monthly?.Max ?? 0);
        };
        client.OnRequest += headers => WriteRequestParams(accessor.HttpContext?.Response, headers);
        client.OnResponseContent += content => FinstatEnvelopeBuilder.CaptureRawBody(accessor.HttpContext, content);
    }

    public static void Attach(CZ::FinstatApi.CommonAbstractApiClient client)
    {
        var accessor = s_accessor;
        if (accessor is null) return;
        client.OnResponse += _ =>
        {
            var l = client.Limits;
            if (l is null) return;
            WriteLimits(accessor.HttpContext?.Response,
                l.Daily?.Current ?? 0, l.Daily?.Max ?? 0,
                l.Monthly?.Current ?? 0, l.Monthly?.Max ?? 0);
        };
        client.OnRequest += headers => WriteRequestParams(accessor.HttpContext?.Response, headers);
        client.OnResponseContent += content => FinstatEnvelopeBuilder.CaptureRawBody(accessor.HttpContext, content);
    }

    private static void WriteLimits(HttpResponse? response, long dc, long dm, long mc, long mm)
    {
        if (response is null || response.HasStarted) return;
        var h = response.Headers;
        h[DailyCurrent]   = dc.ToString();
        h[DailyMax]       = dm.ToString();
        h[MonthlyCurrent] = mc.ToString();
        h[MonthlyMax]     = mm.ToString();
    }

    private static void WriteRequestParams(HttpResponse? response, Dictionary<string, string[]> headers)
    {
        if (response is null || response.HasStarted || headers is null || headers.Count == 0) return;
        var map = new Dictionary<string, string>(headers.Count);
        foreach (var kv in headers)
        {
            if (kv.Value is { Length: > 0 } v) map[kv.Key] = v[0];
        }
        response.Headers[RequestParams] = JsonSerializer.Serialize(map);
    }
}
