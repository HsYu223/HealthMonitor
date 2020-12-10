using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthMonitor.WebApplication.Infrastructure
{
    public static class UrlGroupBuilder
    {
        public static void Build(this IHealthChecksBuilder builder)
        {
            builder
                .AddUrlGroup(new Uri("**"), "**", HealthStatus.Degraded, new[] { "**" }, TimeSpan.FromSeconds(3))
                .AddUrlGroup(new Uri("**"), "**", HealthStatus.Degraded, new[] { "**" }, TimeSpan.FromSeconds(3));
        }
    }
}
