namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Configures the client certificate injected into outbound FinStat client calls when
/// the target URL points at a dev endpoint (<c>zdrojak.eu</c> / <c>zdrojakcz.eu</c> /
/// <c>localhost</c>). Mirrors DesktopFinstatApiTester's <c>ConfigureDevSslHandler</c>.
/// <para>
/// Bound from the <c>Security:DevCertificate</c> configuration section. Local devs set
/// <see cref="Fingerprint"/> in <c>appsettings.Development.json</c>; production will pull
/// the same value from Birko.Security.Vault.
/// </para>
/// </summary>
public sealed class DevCertificateSettings
{
    public const string SectionName = "Security:DevCertificate";

    /// <summary>
    /// SHA-1 thumbprint of the expected client certificate (40 hex chars; spaces/colons
    /// are stripped). When present, the resolver looks up the cert by thumbprint only.
    /// </summary>
    public string? Fingerprint { get; set; }

    /// <summary>
    /// Issuer name used when <see cref="Fingerprint"/> is empty. Default matches the
    /// desktop tester.
    /// </summary>
    public string IssuerName { get; set; } = "root_ca_dev_zdrojak.eu";

    /// <summary>
    /// URL substrings that trigger dev-cert injection. Any outbound URL containing one
    /// of these is considered a dev endpoint.
    /// </summary>
    public string[] DevHosts { get; set; } = ["zdrojak.eu", "zdrojakcz.eu", "localhost"];

    /// <summary>
    /// Absolute path to a PKCS#12/PFX file holding the dev client certificate. Cross-
    /// platform alternative to the Windows cert store — required in Linux containers.
    /// When set, <see cref="DevCertificateResolver"/> loads this file first and falls
    /// back to <see cref="Fingerprint"/> / <see cref="IssuerName"/> lookups only when
    /// the file is missing or fails to load.
    /// </summary>
    public string? PfxPath { get; set; }

    /// <summary>
    /// Password for <see cref="PfxPath"/> and <see cref="PfxBase64"/>. Keep out of
    /// checked-in config — pass via <c>Security__DevCertificate__PfxPassword</c>
    /// env var, an overlay appsettings, or a Vault secret.
    /// </summary>
    public string? PfxPassword { get; set; }

    /// <summary>
    /// Base64-encoded PKCS#12/PFX bytes. Use when you want the certificate to live
    /// entirely in configuration / Vault without a host-side file — <see cref="DevCertificateResolver"/>
    /// prefers this over <see cref="PfxPath"/> when both are set. Paste the output
    /// of <c>base64 -w 0 dev-cert.pfx</c> (or <c>[Convert]::ToBase64String(...)</c>
    /// on PowerShell) into the <c>Security--DevCertificate--PfxBase64</c> Vault key.
    /// </summary>
    public string? PfxBase64 { get; set; }
}
