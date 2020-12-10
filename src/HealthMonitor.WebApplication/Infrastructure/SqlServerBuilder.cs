using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthMonitor.WebApplication.Infrastructure
{
    public static class SqlServerBuilder
    {
        public static void Build(this IHealthChecksBuilder builder)
        {
            builder.AddSqlServer(
                connectionString: "**",
                healthQuery: "SELECT 1;",
                name: "**",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "**" },
                timeout: TimeSpan.FromSeconds(3));
        }
    }
}
