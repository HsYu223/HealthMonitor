只用於站台自我監控, 顯示目前相關服務狀態, 不做其他用途

# Dependent on:
* AspNetCore.HealthChecks.UI
* AspNetCore.HealthChecks.UI.Client
* AspNetCore.HealthChecks.UI.InMemory.Storage
* Microsoft.Owin
* Microsoft.Owin.Host.SystemWeb

# Startup.cs
```csharp
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace HealthMonitor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<HttpClient>(sp =>
                {
                    return new HttpClient(
                        new HttpClientHandler()
                        {
                            Proxy = new WebProxy()
                            {
                                Address = new Uri("**"),
                                BypassProxyOnLocal = false
                            },
                            UseProxy = true
                        });
                })

            services.AddControllers();

            // 健康狀態
            services
                .AddHealthChecksUI(setup =>
                {
                    setup.AddHealthCheckEndpoint(
                        "HealthMonitor",
                        "http://" + Environment.MachineName + ":63846/health");
                    setup.AddWebhookNotification(
                        name: "chat",
                        uri: "http://" + Environment.MachineName + ":63846/message?token={{chat token}}",
                        payload: "{ \"text\": \"" + Environment.MachineName + "  Webhook report for [[LIVENESS]]: [[FAILURE]] - Description: [[DESCRIPTIONS]] \n http://" + Environment.MachineName + ":63846/healthchecks-ui \"}",
                        restorePayload: "{ \"text\": \"" + Environment.MachineName + " [[LIVENESS]] is back to life\"}",
                        shouldNotifyFunc: report =>
                        {
                            return NotifyCheck.GetResult(report);
                        },
                        customMessageFunc: report =>
                        {
                            var failing = report.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy || e.Value.Status == UIHealthStatus.Degraded);
                            return $"{failing.Count()} healthchecks are failing";
                        },
                        customDescriptionFunc: report =>
                        {
                            var failing = report.Entries.Where(e => e.Value.Status == UIHealthStatus.Unhealthy || e.Value.Status == UIHealthStatus.Degraded);
                            return $"HealthChecks with names:  [{string.Join(", ", failing.Select(f => f.Key))}] are failing";
                        });
                })
                .AddInMemoryStorage();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseHealthChecksUI();
        }
    }
}
```

# appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "{{SiteName}}",
        "Uri": "https://{{SiteName}}.evertrust.com.tw/health"
      },
      {
        "Name": "{{SiteName}}",
        "Uri": "https://{{SiteName}}.evertrust.com.tw/health"
      }
    ],
    "EvaluationTimeinSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 30
  }
}
```


正式環境也可以透過參數置換改成正式環境網址

# appsettings.Release.json
```json
{
  "HealthChecksUI": {
    "@jdt.replace": {
      "HealthChecks": [
        {
          "Name": "{{SiteName}}",
          "Uri": "https://{{SiteName}}.ycut.com.tw/health"
        }
      ],
      "EvaluationTimeinSeconds": 10,
      "MinimumSecondsBetweenFailureNotifications": 30
    }
  }
}
```
