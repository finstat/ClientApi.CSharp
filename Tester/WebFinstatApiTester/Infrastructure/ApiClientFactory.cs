extern alias CZ;

using FinstatApi;

namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Builds per-request FinStat API clients from <see cref="FinstatCredentials"/>.
/// A fresh client per request is cheap — the underlying implementation holds only settings,
/// no pooled connections.
/// <para>
/// Every client is wired through <see cref="FinstatLimitsHeaders.Attach(FinstatApi.CommonAbstractApiClient)"/>
/// so the browser sees the daily/monthly quota counters as response headers on each call,
/// and through <see cref="DevCertificateResolver.Attach(FinstatApi.CommonAbstractApiClient, string)"/>
/// which installs the dev client certificate when the outbound URL hits zdrojak.eu.
/// </para>
/// </summary>
internal static class ApiClientFactory
{
    // ── SK ──
    public static ApiClient Sk(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiMonitoringClient SkMonitoring(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiMonitoringClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiDistraintClient SkDistraint(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiDistraintClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiDailyDiffClient SkDailyDiff(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiDailyDiffClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiDailyStatementDiffClient SkDailyStatementDiff(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiDailyStatementDiffClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiDailyStatement2014DiffClient SkDailyStatement2014Diff(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiDailyStatement2014DiffClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiDailyUltimateDiffClient SkDailyUltimateDiff(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiDailyUltimateDiffClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiBankruptcyRestructuringClient SkBankruptcyRestructuring(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiBankruptcyRestructuringClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiStatementClient SkStatement(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiStatementClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    public static ApiReportingClient SkReporting(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new ApiReportingClient(c.UrlSk, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlSk);
        return client;
    }

    // ── CZ ──
    public static CZ::FinstatApi.ApiClient Cz(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new CZ::FinstatApi.ApiClient(c.UrlCz, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlCz);
        return client;
    }

    public static CZ::FinstatApi.ApiMonitoringClient CzMonitoring(FinstatCredentials c)
    {
        c.EnsureValid();
        var client = new CZ::FinstatApi.ApiMonitoringClient(c.UrlCz, c.ApiKey, c.PrivateKey, c.StationID, c.StationName, c.TimeOut);
        FinstatLimitsHeaders.Attach(client);
        DevCertificateResolver.Attach(client, c.UrlCz);
        return client;
    }
}
