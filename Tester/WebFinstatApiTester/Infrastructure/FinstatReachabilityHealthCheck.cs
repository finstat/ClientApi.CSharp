using Birko.Health;

namespace WebFinstatApiTester.Infrastructure;

public sealed class FinstatReachabilityHealthCheck(IHttpClientFactory http, string url) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckAsync(CancellationToken ct)
    {
        using var client = http.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);
        try
        {
            using var resp = await client.GetAsync(url, ct);
            var code = (int)resp.StatusCode;
            return code < 500
                ? HealthCheckResult.Healthy($"{url} reachable ({code})")
                : HealthCheckResult.Degraded($"{url} returned {code}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    }
}
