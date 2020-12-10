using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using HealthMonitor.WebApplication.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace HealthMonitor.WebApplication
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
                .AddSingleton(sp =>
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
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "HealthMonitor",
                    Version = "v1"
                });
            });

            services.AddControllers();

            // °·±dª¬ºA
            services
                .AddHealthChecksUI(setup =>
                {
                    var domain = "http://localhost:51822";
                    setup.AddHealthCheckEndpoint(
                        "HealthMonitor",
                        $"{domain}/health");
                    setup.AddWebhookNotification(
                        name: "chat",
                        uri: $"{domain}/api/message?token={{token}}",
                        payload: "{ \"text\": \"" + Environment.MachineName + "  Webhook report for [[LIVENESS]]: [[FAILURE]] - Description: [[DESCRIPTIONS]] \n " + domain + "/healthchecks-ui \"}",
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

            var healthCheckBuilder = services.AddHealthChecks();
            SqlServerBuilder.Build(healthCheckBuilder);
            UrlGroupBuilder.Build(healthCheckBuilder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI();
        }
    }
}
