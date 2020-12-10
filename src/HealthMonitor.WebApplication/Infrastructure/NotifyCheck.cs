using System;
using System.Collections.Generic;
using System.Linq;
using HealthChecks.UI.Core;

namespace HealthMonitor.WebApplication.Infrastructure
{
    public static class NotifyCheck
    {
        public static UIHealthStatus Status = UIHealthStatus.Healthy;

        public static Dictionary<string, UIHealthReportEntry> Entries = new Dictionary<string, UIHealthReportEntry>();

        public static bool ShouldNotify = false;

        public static bool GetResult(UIHealthReport report)
        {
            var shouldNotify = false;

            if (DateTime.Now.Hour < 7)
            {
                return false;
            }

            if (report != null && report.Status != UIHealthStatus.Healthy)
            {
                if (Status != UIHealthStatus.Healthy)
                {
                    var lastWarring = Entries.Select(e => e.Key);

                    if (report.Entries.Where(e => e.Value.Status != UIHealthStatus.Healthy && lastWarring.Contains(e.Key)).Any())
                    {
                        Entries = report.Entries.Where(e => e.Value.Status != UIHealthStatus.Healthy).ToDictionary(e => e.Key, e => e.Value);
                        shouldNotify = true;
                        ShouldNotify = shouldNotify;
                    }
                    else
                    {
                        Status = report.Status;
                        Entries = report.Entries.Where(e => e.Value.Status != UIHealthStatus.Healthy).ToDictionary(e => e.Key, e => e.Value);
                        shouldNotify = false;
                        ShouldNotify = shouldNotify;
                    }
                }
                else
                {
                    Status = report.Status;
                    Entries = report.Entries.Where(e => e.Value.Status != UIHealthStatus.Healthy).ToDictionary(e => e.Key, e => e.Value);
                    shouldNotify = false;
                    ShouldNotify = shouldNotify;
                }
            }
            else
            {
                if (ShouldNotify)
                {
                    shouldNotify = ShouldNotify;
                }

                Status = UIHealthStatus.Healthy;
                Entries = new Dictionary<string, UIHealthReportEntry>();
                ShouldNotify = false;
            }

            return shouldNotify;
        }
    }
}
