using CloudTek.Build.Packaging;
using Nuke.Common;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

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
        Log.Information($"Packing NuGet packages...");

        PackageManager.Pack(Repository, this, VersioningStrategy, Configuration);
      });

  /// <summary>
  /// dotnet nuke --target VulnerableScan
  /// Run dotnet native listing for vulnerable packages and report it to ApplicationInsights
  /// </summary>
  protected virtual Target VulnerableScan => _ => _
    .Description(
      "Run dotnet list for vulnerable packages and report it to ApplicationInsights")
    .Before(RunChecks)
    .Executes(
      () =>
      {
        var outputs = DotNet(
          "list package --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json",
          Solution.Directory);

        if (outputs != null)
        {
          VulnerabilityScanner.ReportScan(this, Repository.Name, outputs);
        }
      });

  /// <summary>
  /// dotnet nuke --target BuildDependencyTree
  /// Determines the dependencies of the soltion for BETA/Outdated/Vulnerability checks
  /// </summary>
  protected virtual Target BuildDependencyTree => _ => _
    .DependsOn(Restore, DotnetOutdatedInstall)
    .Executes(() => { PackageManager.BuildDependencyTree(this); });
}