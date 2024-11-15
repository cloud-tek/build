using CloudTek.Build.Packaging;
using Nuke.Common;
using Serilog;

namespace CloudTek.Build;

public abstract partial class SmartBuild : NukeBuild
{
  /// <summary>
  /// dotnet nuke --target Restore
  /// </summary>
  protected virtual Target Restore => _ => _
    .DependsOn(Clean)
    .Executes(
      () =>
      {
        PackageManager.Restore(this);

        SolutionRestored = true;
      });

  /// <summary>
  /// dotnet nuke --target Pack
  /// Packs the artifacts with the type equal to 'Package'
  /// </summary>
  protected virtual Target Pack => _ => _
    .Description("Pack packable projects into NuGet packages and put them in artifacts/packages directory")
    .After(Test)
    .DependsOn(Compile)
    .Executes(
      () =>
      {
        Log.Information($"Packing NuGet packages ...");

        PackageManager.Pack(Repository, this, VersioningStrategy, Configuration);
      });

  /// <summary>
  /// dotnet nuke --target VulnerableScan
  /// Runs dotnet list package --vulnerable and reports results via telemetry
  /// </summary>
  protected virtual Target Scan => _ => _
    .Description(
      "Performs 'dotnet list package --vulnerable' and reports results via telemetry")
    .Before(RunChecks)
    .Executes(
      () =>
      {
        Log.Information($"Scanning dependencies for vulnerabilities ...");

        VulnerabilityScanner.Scan(this);
      });

  /// <summary>
  /// dotnet nuke --target BuildDependencyTree
  /// Determines the dependencies of the soltion for BETA/Outdated/Vulnerability checks
  /// </summary>
  protected virtual Target BuildDependencyTree => _ => _
    .DependsOn(Restore, DotnetOutdatedInstall)
    .Executes(() => { PackageManager.BuildDependencyTree(this); });
}