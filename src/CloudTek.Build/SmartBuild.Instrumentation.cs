using System.Diagnostics;
using System.Diagnostics.Metrics;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace CloudTek.Build;

public partial class SmartBuild
{
  private const string ConnectionString = "InstrumentationKey=c28d10d1-b31e-4d2e-a22b-8bc733a85cf4;IngestionEndpoint=https://swedencentral-0.in.applicationinsights.azure.com/;ApplicationId=6c27684a-8a28-48e8-86d8-b9c60a98d8e4";

  /// <summary>
  /// The OpenTelemetry Meter
  /// </summary>
  public static readonly Meter Meter = new("CloudTek.Build");

  private static readonly ResourceBuilder ResourceBuilder = ResourceBuilder.CreateDefault().AddAttributes(
    new Dictionary<string, object> { { "service.name", IsLocalBuild ? "DeveloperMachine" : "BuildAgent" } });

  /// <summary>
  /// The LoggerFactory
  /// </summary>
  public static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
    builder =>
    {
      builder.AddOpenTelemetry(
        options =>
        {
          options.SetResourceBuilder(ResourceBuilder);
          options.AddAzureMonitorLogExporter(configure => { configure.ConnectionString = ConnectionString; });
        });
    });

  private readonly Dictionary<string, Stopwatch> _stopwatches = new();

  internal readonly MeterProvider MetricsProvider = Sdk.CreateMeterProviderBuilder()
    .AddMeter(Meter.Name)
    .SetResourceBuilder(ResourceBuilder)
    .AddAzureMonitorMetricExporter(
      configure =>
      {
        configure.ConnectionString = ConnectionString;
      }).Build();

  internal readonly Histogram<long> TargetDuration = Meter.CreateHistogram<long>("TargetDuration");

  internal void ReportDuration(string target, string repository, bool succeed, long elapsedMilliseconds)
  {
    TargetDuration.Record(
      elapsedMilliseconds,
      new KeyValuePair<string, object?>("Target", target),
      new KeyValuePair<string, object?>("Repository", repository),
      new KeyValuePair<string, object?>("Succeed", succeed));
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
    ReportDuration(target, GitRepositoryName, true, _stopwatches[target].ElapsedMilliseconds);
  }

  /// <summary>
  /// When a target fails, report duration
  /// </summary>
  /// <param name="target"></param>
  protected override void OnTargetFailed(string target)
  {
    base.OnTargetFailed(target);
    _stopwatches[target].Stop();
    ReportDuration(target, GitRepositoryName, false, _stopwatches[target].ElapsedMilliseconds);
  }
}