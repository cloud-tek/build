using System.Diagnostics;
using System.Diagnostics.Metrics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using Nuke.Common;
using Nuke.Common.Utilities;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace CloudTek.Build;

public partial class SmartBuild
{
  /// <summary>
  /// The OTEL Meter
  /// </summary>
  private static readonly Meter Meter = new("CloudTek.Build");

  private static readonly ResourceBuilder ResourceBuilder = ResourceBuilder
    .CreateDefault()
    .AddAttributes(new Dictionary<string, object>
    {
      { "service.name", IsLocalBuild ? "DeveloperMachine" : "BuildAgent" }
    });

  private readonly Dictionary<string, Stopwatch> _stopwatches = new();

  internal readonly Histogram<long> TargetDuration = Meter.CreateHistogram<long>("TargetDuration");

  /// <summary>
  /// The LoggerFactory
  /// </summary>
  internal ILoggerFactory LoggerFactory = default!;

  /// <summary>
  /// The Metrics provider
  /// </summary>
  private MeterProvider MetricsProvider = default!;

  /// <summary>
  /// Parameter used for providing appinsights connection string for telemetry
  /// </summary>
  [Parameter("Application insights connection string")]
  public virtual string? AppInsightsConnectionString { get; set; } = string.Empty;

  private void InitializeTelemetry()
  {
    if (!AppInsightsConnectionString.IsNullOrEmpty())
    {
      LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
        builder =>
        {
          builder.AddOpenTelemetry(
            options =>
            {
              options.SetResourceBuilder(ResourceBuilder);
              options.AddAzureMonitorLogExporter(
                configure =>
                {
                  configure.ConnectionString = AppInsightsConnectionString;
                });
            });
        });

      MetricsProvider = Sdk.CreateMeterProviderBuilder()
        .AddMeter(Meter.Name)
        .SetResourceBuilder(ResourceBuilder)
        .AddAzureMonitorMetricExporter(
          configure =>
          {
            configure.ConnectionString = AppInsightsConnectionString;
          }).Build();
    }
  }

  internal void ReportDuration(string target, string repository, bool success, long elapsedMilliseconds)
  {
    TargetDuration.Record(
      elapsedMilliseconds,
      new KeyValuePair<string, object?>("Target", target),
      new KeyValuePair<string, object?>("Repository", repository),
      new KeyValuePair<string, object?>("Success", success));
  }

  /// <summary>
  /// When a target starts, add a stopwatch to measure execution
  /// </summary>
  /// <param name="target"></param>
  protected override void OnTargetRunning(string target)
  {
    _stopwatches.Add(target, Stopwatch.StartNew());
    base.OnTargetRunning(target);
  }

  /// <summary>
  /// When a target is successful, report duration
  /// </summary>
  /// <param name="target"></param>
  protected override void OnTargetSucceeded(string target)
  {
    base.OnTargetSucceeded(target);
    _stopwatches[target].Stop();
    ReportDuration(target, Repository.Name, true, _stopwatches[target].ElapsedMilliseconds);
  }

  /// <summary>
  /// When a target fails, report duration
  /// </summary>
  /// <param name="target"></param>
  protected override void OnTargetFailed(string target)
  {
    base.OnTargetFailed(target);
    _stopwatches[target].Stop();
    ReportDuration(target, Repository.Name, false, _stopwatches[target].ElapsedMilliseconds);
  }
}