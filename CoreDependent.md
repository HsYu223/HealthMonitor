只用於站台自我監控, 顯示目前相關服務狀態, 不做其他用途

# Dependent on:
* AspNetCore.HealthChecks.Aws.S3
* AspNetCore.HealthChecks.Hangfire
* AspNetCore.HealthChecks.MongoDb
* AspNetCore.HealthChecks.Network
* AspNetCore.HealthChecks.Rabbitmq
* AspNetCore.HealthChecks.Redis
* AspNetCore.HealthChecks.SqlServer
* AspNetCore.HealthChecks.Uris
* Microsoft.Extensions.Diagnostics.HealthChecks
* Microsoft.Extensions.Diagnostics.HealthChecks.Abstractions

# Startup.cs
```csharp
using System;
using System.Net;
using System.Net.Http;
using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

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
                });

            services.AddControllers();

            // 健康狀態
            services.AddHealthChecks()
                .AddSqlServer(
                    sp =>
                    {
                        var connClient = sp.GetRequiredService<*>();
                        var connectionString = connClient.GetConnectionString("*");

                        return connectionString;
                    },
                    healthQuery: "SELECT 1;",
                    name: "* DB",
                    failureStatus: HealthStatus.Degraded, null, TimeSpan.FromSeconds(3))
                .AddRedis(
                    "redis://*",
                    "redis", HealthStatus.Degraded, null, TimeSpan.FromSeconds(3))
                .AddRabbitMQ(
                    sp =>
                    {
                        var factory = new ConnectionFactory()
                        {
                            //設定連線 RabbitMQ username
                            UserName = "XXXX",
                            //設定 RabbitMQ password
                            Password = "XXXX",
                            //自動回復連線
                            AutomaticRecoveryEnabled = true,
                            //心跳檢測頻率
                            RequestedHeartbeat = 10,
                            VirtualHost = "XXXX"
                        };
                        var connection = factory.CreateConnection(
                            AmqpTcpEndpoint.ParseMultiple("*"));

                        return connection;
                    }, "rabbitMQ", HealthStatus.Degraded, null, TimeSpan.FromSeconds(3))
                .AddS3(
                    setup =>
                    {
                        setup.AccessKey = "XXXXX";
                        setup.BucketName = "XXXX";
                        setup.SecretKey = "XXXXX";
                        setup.S3Config = new AmazonS3Config()
                        {
                            ServiceURL = "https://XXX.amazonaws.com",
                            ForcePathStyle = true,
                            UseAccelerateEndpoint = false,
                            SignatureVersion = "4",
                            RegionEndpoint = RegionEndpoint.APNortheast1
                        };
                    })
                .AddUrlGroup(
                    new Uri("**"), "weburl services", HealthStatus.Degraded, null, TimeSpan.FromSeconds(3))
                .AddUrlGroup(
                    sp =>
                    {
                        var urlClient = sp.GetRequiredService<*>();
                        var empinfoUrl = urlClient.GetUrl(*);

                        return new Uri($"{empinfoUrl}/swagger/ui/index");
                    }, "empinfo services", HealthStatus.Degraded, null, TimeSpan.FromSeconds(3))
                .AddUrlGroup(
                    sp =>
                    {
                        var urlClient = sp.GetRequiredService<*>();
                        var deptinfoUrl = urlClient.GetUrl(*);

                        return new Uri($"{deptinfoUrl}/swagger");
                    }, "deptinfo services", HealthStatus.Degraded, null, TimeSpan.FromSeconds(3))
                .AddUrlGroup(
                    sp =>
                    {
                        var urlClient = sp.GetRequiredService<*>();
                        var authUrl = urlClient.GetUrl("*");

                        return new Uri($"{authUrl}/.well-known/openid-configuration");
                    }, "auth services", HealthStatus.Degraded, null, TimeSpan.FromSeconds(3))
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

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        }
    }
}
```
