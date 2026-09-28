using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// <see cref="HttpClientHandler"/> subclass that writes one line to stdout per outbound
/// request — method, URL (minus query string to keep secrets out of logs), response
/// status, and elapsed milliseconds. Landing on stdout means <c>docker logs</c> picks
/// it up directly without any provider configuration.
/// <para>
/// Used by <see cref="DevCertificateResolver"/> for dev-host calls; non-dev traffic
/// keeps the FinstatApi client's default handler.
/// </para>
/// </summary>
internal sealed class LoggingHttpClientHandler : HttpClientHandler
{
    private const string LogPrefix = "Finstat";

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var target = SafeUri(request.RequestUri);
        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            Console.WriteLine(
                $"{LogPrefix}: {request.Method} {target} -> {(int)response.StatusCode} {response.ReasonPhrase} in {sw.ElapsedMilliseconds}ms");
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"{LogPrefix}: {request.Method} {target} FAILED in {sw.ElapsedMilliseconds}ms — {ex.GetType().Name}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Strip query string — FinStat auth hashes and api keys live there. Keep the path
    /// which is the valuable bit for debugging.
    /// </summary>
    private static string SafeUri(Uri? uri)
    {
        if (uri is null) return "(null)";
        return uri.GetLeftPart(UriPartial.Path);
    }
}
