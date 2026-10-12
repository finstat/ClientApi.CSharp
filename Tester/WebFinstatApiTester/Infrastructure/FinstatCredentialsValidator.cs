using Birko.Validation.Fluent;

namespace WebFinstatApiTester.Infrastructure;

public sealed class FinstatCredentialsValidator : AbstractValidator<FinstatCredentials>
{
    public FinstatCredentialsValidator()
    {
        RuleFor(c => c.ApiKey).Required().Length(8, 128);
        RuleFor(c => c.PrivateKey).Required().Length(8, 256);
        RuleFor(c => c.StationID).Required().MaxLength(64);
        RuleFor(c => c.StationName).Required().MaxLength(128);
        RuleFor(c => c.TimeOut).GreaterThanOrEqual(0);
        RuleFor(c => c.UrlSk).Required().Must(IsHttpUrl, "Must be an http(s) URL.");
        RuleFor(c => c.UrlCz).Required().Must(IsHttpUrl, "Must be an http(s) URL.");
        RuleFor(c => c.Lang).Must(l => l is null or "sk" or "cs", "Must be sk or cs.");
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var u) &&
        (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
}
