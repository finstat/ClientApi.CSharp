extern alias CZ;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using FinstatApi;

namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Ports DesktopFinstatApiTester's <c>ConfigureDevSslHandler</c> to the web tester.
/// When the outbound URL hits a dev host (<see cref="DevCertificateSettings.DevHosts"/>),
/// installs an <see cref="HttpClientHandler"/> that (a) attaches a client certificate
/// resolved from the Windows cert store and (b) accepts any server certificate — matching
/// what the desktop tester does under <c>#if DEBUG</c>.
/// <para>
/// Prefers lookup by <see cref="DevCertificateSettings.Fingerprint"/> when set (from
/// <c>appsettings.Development.json</c> or the vault); otherwise falls back to a
/// <see cref="X509FindType.FindByIssuerName"/> match against
/// <see cref="DevCertificateSettings.IssuerName"/> (desktop behaviour).
/// </para>
/// <para>
/// Console.WriteLine statements follow the Birko convention (<c>"LocalVault: …"</c>):
/// short prefix + sentence. They land on stdout in the container → <c>docker logs</c>,
/// and are captured by ASP.NET's default logger provider in dev runs.
/// </para>
/// </summary>
internal static class DevCertificateResolver
{
    private const string LogPrefix = "DevCert";

    private static DevCertificateSettings s_settings = new();
    private static X509Certificate2? s_cachedCert;

    /// <summary>
    /// Apply configuration and eagerly resolve the certificate so startup logs surface
    /// the outcome. Subsequent calls to <see cref="Attach"/> reuse the cached instance.
    /// </summary>
    public static void UseSettings(DevCertificateSettings settings)
    {
        s_settings = settings ?? new DevCertificateSettings();
        // Deduplicate DevHosts — the IConfiguration array binder APPENDS configured
        // entries to the C# default (["zdrojak.eu", …]) instead of replacing, so when
        // appsettings.json restates the same hosts we end up with 6 entries.
        s_settings.DevHosts = s_settings.DevHosts
            .Where(h => !string.IsNullOrEmpty(h))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        LogConfiguration();

        // Dispose any previously-cached certificate (e.g. reload during tests) before we replace it.
        s_cachedCert?.Dispose();
        s_cachedCert = ResolveCertificate();

        if (s_cachedCert is not null)
        {
            Console.WriteLine(
                $"{LogPrefix}: certificate ready — thumbprint={s_cachedCert.Thumbprint}, " +
                $"subject='{s_cachedCert.Subject}', hasPrivateKey={s_cachedCert.HasPrivateKey}, " +
                $"notAfter={s_cachedCert.NotAfter:u}");
        }
        else
        {
            Console.WriteLine(
                $"{LogPrefix}: no certificate resolved. Outbound calls to dev hosts " +
                $"({string.Join(", ", s_settings.DevHosts)}) will fail mutual TLS.");
        }
    }

    public static void Attach(FinstatApi.CommonAbstractApiClient client, string url)
    {
        // Always install a logging handler so every outbound FinStat call lands in
        // the container log. Only dev URLs additionally get the client certificate
        // + lenient server-cert validation — production traffic uses the system
        // trust store as normal.
        client.HttpClientHandlerFactory = BuildHandlerFactory(IsDevUrl(url));
    }

    public static void Attach(CZ::FinstatApi.CommonAbstractApiClient client, string url)
    {
        client.HttpClientHandlerFactory = BuildHandlerFactory(IsDevUrl(url));
    }

    public static bool IsDevUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        foreach (var host in s_settings.DevHosts)
        {
            if (!string.IsNullOrEmpty(host) && url.Contains(host, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static Func<HttpClientHandler> BuildHandlerFactory(bool isDev)
    {
        return () =>
        {
            var handler = new LoggingHttpClientHandler();
            if (isDev)
            {
                // Dev servers often use untrusted certs — accept any server cert and
                // attach the cached dev client cert for mutual TLS.
                handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
                if (s_cachedCert is not null)
                {
                    handler.ClientCertificates.Add(s_cachedCert);
                }
            }
            return handler;
        };
    }

    private static void LogConfiguration()
    {
        var s = s_settings;
        Console.WriteLine(
            $"{LogPrefix}: config DevHosts=[{string.Join(", ", s.DevHosts)}] " +
            $"Fingerprint={Short(s.Fingerprint, "<unset>")} " +
            $"IssuerName='{s.IssuerName}' " +
            $"PfxBase64={(string.IsNullOrEmpty(s.PfxBase64) ? "<unset>" : $"<{s.PfxBase64.Length} chars>")} " +
            $"PfxPath={(string.IsNullOrEmpty(s.PfxPath) ? "<unset>" : s.PfxPath)} " +
            $"PfxPassword={(string.IsNullOrEmpty(s.PfxPassword) ? "<unset>" : "<set>")}");
    }

    private static string Short(string? value, string fallback)
    {
        if (string.IsNullOrEmpty(value)) return fallback;
        return value.Length <= 16 ? value : $"{value[..8]}…{value[^4..]}";
    }

    private static X509Certificate2? ResolveCertificate()
        => FindByPfxBase64() ?? FindByPfxFile() ?? FindByFingerprint() ?? FindByIssuer();

    private static X509Certificate2? FindByPfxBase64()
    {
        var b64 = s_settings.PfxBase64;
        if (string.IsNullOrWhiteSpace(b64)) return null;

        byte[] bytes;
        try { bytes = Convert.FromBase64String(b64.Trim()); }
        catch (FormatException ex)
        {
            Console.WriteLine($"{LogPrefix}: PfxBase64 is not valid base64 — {ex.Message}");
            return null;
        }

        X509Certificate2? cert;
        try
        {
            cert = string.IsNullOrEmpty(s_settings.PfxPassword)
                ? X509CertificateLoader.LoadPkcs12(bytes, null)
                : X509CertificateLoader.LoadPkcs12(bytes, s_settings.PfxPassword);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{LogPrefix}: PfxBase64 failed to load — {ex.GetType().Name}: {ex.Message} (wrong password?)");
            return null;
        }

        var expected = NormalizeFingerprint(s_settings.Fingerprint ?? string.Empty);
        if (!string.IsNullOrEmpty(expected)
            && !string.Equals(cert.Thumbprint, expected, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(
                $"{LogPrefix}: PfxBase64 thumbprint mismatch — expected={expected}, actual={cert.Thumbprint}. " +
                $"Verify Fingerprint matches the PFX bytes in config.");
            cert.Dispose();
            return null;
        }

        Console.WriteLine($"{LogPrefix}: loaded cert from PfxBase64 (config-embedded)");
        return cert;
    }

    private static X509Certificate2? FindByPfxFile()
    {
        var path = s_settings.PfxPath;
        if (string.IsNullOrWhiteSpace(path)) return null;
        if (!File.Exists(path))
        {
            Console.WriteLine($"{LogPrefix}: PfxPath '{path}' does not exist — skipping.");
            return null;
        }

        X509Certificate2? cert;
        try
        {
            cert = string.IsNullOrEmpty(s_settings.PfxPassword)
                ? X509CertificateLoader.LoadPkcs12FromFile(path, null)
                : X509CertificateLoader.LoadPkcs12FromFile(path, s_settings.PfxPassword);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{LogPrefix}: PfxPath '{path}' failed to load — {ex.GetType().Name}: {ex.Message} (wrong password?)");
            return null;
        }

        // Defence-in-depth: when Fingerprint is configured alongside PfxPath, reject
        // any loaded cert whose thumbprint doesn't match. Prevents the wrong PFX being
        // silently picked up from a misconfigured bind mount.
        var expected = NormalizeFingerprint(s_settings.Fingerprint ?? string.Empty);
        if (!string.IsNullOrEmpty(expected)
            && !string.Equals(cert.Thumbprint, expected, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine(
                $"{LogPrefix}: PfxPath '{path}' thumbprint mismatch — expected={expected}, actual={cert.Thumbprint}.");
            cert.Dispose();
            return null;
        }

        Console.WriteLine($"{LogPrefix}: loaded cert from PfxPath '{path}'");
        return cert;
    }

    private static X509Certificate2? FindByFingerprint()
    {
        var fingerprint = s_settings.Fingerprint;
        if (string.IsNullOrWhiteSpace(fingerprint)) return null;
        var normalized = NormalizeFingerprint(fingerprint);
        var cert = FindBy(X509FindType.FindByThumbprint, normalized);
        if (cert is not null)
            Console.WriteLine($"{LogPrefix}: loaded cert from Windows store (fingerprint match)");
        else
            Console.WriteLine($"{LogPrefix}: no cert with fingerprint={normalized} in Windows store (CurrentUser+LocalMachine)");
        return cert;
    }

    private static X509Certificate2? FindByIssuer()
    {
        if (string.IsNullOrWhiteSpace(s_settings.IssuerName)) return null;
        var cert = FindBy(X509FindType.FindByIssuerName, s_settings.IssuerName);
        if (cert is not null)
            Console.WriteLine($"{LogPrefix}: loaded cert from Windows store (issuer='{s_settings.IssuerName}')");
        else
            Console.WriteLine($"{LogPrefix}: no cert with issuer='{s_settings.IssuerName}' in Windows store");
        return cert;
    }

    private static X509Certificate2? FindBy(X509FindType type, object value)
    {
        foreach (var location in new[] { StoreLocation.CurrentUser, StoreLocation.LocalMachine })
        {
            X509Store? store = null;
            try
            {
                store = new X509Store(StoreName.My, location);
                store.Open(OpenFlags.ReadOnly);
                var matches = store.Certificates.Find(type, value, validOnly: false);
                if (matches.Count > 0) return matches[0];
            }
            catch
            {
                // Store may not exist on non-Windows hosts — fall through to next location.
            }
            finally
            {
                store?.Close();
            }
        }
        return null;
    }

    private static string NormalizeFingerprint(string fingerprint)
        => fingerprint.Replace(" ", "").Replace(":", "").Replace("-", "").ToUpperInvariant();
}
