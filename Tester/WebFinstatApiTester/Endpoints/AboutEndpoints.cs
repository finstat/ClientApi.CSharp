extern alias CZ;

using System.Reflection;

namespace WebFinstatApiTester.Endpoints;

/// <summary>
/// Versions for the About modal, read from the assemblies actually loaded — the tester itself and
/// the SK/CZ FinStat client libraries it calls — so the modal never shows a hand-maintained number.
/// </summary>
public static class AboutEndpoints
{
    public static IEndpointRouteBuilder MapAboutEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/about", () => Results.Ok(new
        {
            tester   = VersionOf(typeof(AboutEndpoints).Assembly),
            clientSk = VersionOf(typeof(FinstatApi.ApiClient).Assembly),
            clientCz = VersionOf(typeof(CZ::FinstatApi.ApiClient).Assembly),
            runtime  = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
        }));
        return routes;
    }

    /// <summary>Informational version without the "+commit" build metadata the SDK appends.</summary>
    private static string VersionOf(Assembly assembly)
    {
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                      ?? assembly.GetName().Version?.ToString()
                      ?? "?";
        var plus = version.IndexOf('+');
        return plus >= 0 ? version[..plus] : version;
    }
}
