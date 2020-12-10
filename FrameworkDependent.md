只用於站台自我監控, 顯示目前相關服務狀態, 不做其他用途

# Dependent on:
* App.Metrics.Concurrency
* App.Metrics.Health
* App.Metrics.Health.Abstractions
* App.Metrics.Health.Checks.Http
* App.Metrics.Health.Checks.Network
* App.Metrics.Health.Checks.Process
* App.Metrics.Health.Checks.Sql
* App.Metrics.Health.Core
* App.Metrics.Health.Extensions.DependencyInjection
* App.Metrics.Health.Formatters.Ascii
* App.Metrics.Health.Formatters.Json
* Microsoft.Owin
* Microsoft.Owin.Host.SystemWeb

# Startup.cs

```csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using App.Metrics.Health;
using App.Metrics.Health.Builder;
using App.Metrics.Health.Checks.Sql;
using App.Metrics.Health.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;


[assembly: OwinStartup(typeof(Startup))]

/// <summary>
/// Startup
/// </summary>
public class Startup
{
	/// <summary>
	/// The client
	/// </summary>
	public static HttpClient _client = new HttpClient();

	/// <summary>
	/// Configurations the specified application.
	/// </summary>
	/// <param name="app">The application.</param>
	public void Configuration(IAppBuilder app)
	{
		// 如需如何設定應用程式的詳細資訊，請瀏覽 https://go.microsoft.com/fwlink/?LinkID=316888
		app.Map("/health", coreapp =>
		{
			var services = new ServiceCollection();
			var healthBuilder = new HealthBuilder()
				.HealthChecks.AddSqlCachedCheck(
					"** DB",
					() => new SqlConnection(ConfigurationManager.ConnectionStrings["**"].ToString()),
					TimeSpan.FromSeconds(10),
					TimeSpan.FromMinutes(1),
					true)
				.HealthChecks.AddHttpGetCheck("**", new Uri(""), TimeSpan.FromSeconds(10), true)
				.BuildAndAddTo(services);

			services.AddHealth(healthBuilder);

			var provider = services.BuildServiceProvider();
			var healthCheckRunner = provider.GetRequiredService<IRunHealthChecks>();

			coreapp.Run(async context =>
			{
				context.Response.ContentType = "application/json;";
				var healthStatus = await healthCheckRunner.ReadAsync();

				var setting = new JsonSerializerSettings()
				{
					Converters = new List<JsonConverter>()
					{
						new StringEnumConverter()
					}
				};

				var entries = healthStatus.Results.Select(
						rows => new KeyValuePair<string, UIHealthReportEntry>(
							rows.Name,
							new UIHealthReportEntry
							{
								Data = new Dictionary<string, object>(),
								Duration = TimeSpan.Zero,
								Status = HealthStatusConvert(rows.Check.Status),
								Tags = new List<string>()
							})).ToDictionary(rows => rows.Key, rows => rows.Value);

				var data = new HealthCheck
				{
					Status = HealthStatusConvert(healthStatus.Status),
					TotalDuration = TimeSpan.Zero,
					Entries = entries
				};

				var result = JsonConvert.SerializeObject(
					data,
					Formatting.None,
					setting);
			
				await context.Response.WriteAsync(result);
			});
		});
	}
}
```

# 其他Model
```csharp
/// <summary>
/// Healthes the status convert.
/// </summary>
/// <returns></returns>
public UIHealthStatus HealthStatusConvert(HealthCheckStatus status)
{
	var reuslt = UIHealthStatus.Degraded;
	switch (status)
	{
		case HealthCheckStatus.Healthy:
			reuslt = UIHealthStatus.Healthy;
			break;
		case HealthCheckStatus.Degraded:
			reuslt = UIHealthStatus.Degraded;
			break;
		case HealthCheckStatus.Unhealthy:
			reuslt = UIHealthStatus.Unhealthy;
			break;
		case HealthCheckStatus.Ignored:
			reuslt = UIHealthStatus.Degraded;
			break;
		default:
			reuslt = UIHealthStatus.Degraded;
			break;
	}

	return reuslt;
}

/// <summary>
/// HealthCheck
/// </summary>
public class HealthCheck
{
	/// <summary>
	/// Status
	/// </summary>
	public UIHealthStatus Status { get; set; }

	/// <summary>
	/// Total Duration
	/// </summary>
	public TimeSpan TotalDuration { get; set; }

	/// <summary>
	/// Entries
	/// </summary>
	public Dictionary<string, UIHealthReportEntry> Entries { get; set; }
}

/// <summary>
/// UIHealthReportEntry
/// </summary>
public class UIHealthReportEntry
{
	public IReadOnlyDictionary<string, object> Data { get; set; }
	public TimeSpan Duration { get; set; }
	public UIHealthStatus Status { get; set; }
	public IEnumerable<string> Tags { get; set; }
}

/// <summary>
/// UIHealthStatus
/// </summary>
public enum UIHealthStatus
{
	Unhealthy = 0,
	Degraded = 1,
	Healthy = 2
}
```
