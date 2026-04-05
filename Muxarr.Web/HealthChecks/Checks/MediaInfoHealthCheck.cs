using Microsoft.Extensions.Diagnostics.HealthChecks;
using Muxarr.Core.Utilities;

namespace Muxarr.Web.HealthChecks.Checks;

public class MediaInfoHealthCheck(ILogger<MediaInfoHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ProcessExecutor.ExecuteProcessAsync("mediainfo", "--Version", TimeSpan.FromSeconds(5));

            if (result.ExitCode == 0)
            {
                return HealthCheckResult.Healthy();
            }

            return HealthCheckResult.Unhealthy($"mediainfo exited with code {result.ExitCode}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed for mediainfo");
            return HealthCheckResult.Unhealthy("mediainfo is not available", ex);
        }
    }
}
