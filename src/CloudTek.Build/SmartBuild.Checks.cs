using System.IO;
using CloudTek.Build.Packaging;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build
{
  public abstract partial class SmartBuild
  {
    private static readonly string[] DefaultHuskyArgs =
      new string[] { "husky", "run", "--name", "commit-message-linter-ci", "--args", "origin/main.." };

    /// <summary>
    /// dotnet nuke --target RunChecks
    /// </summary>
    protected virtual Target RunChecks => _ => _
      .Description("Run all checks")
      .DependsOn(CommitCheck, FormatCheck, FormatAnalyzersCheck, BetaCheck, OutdatedCheck)
      .Before(UnitTests)
      .Executes(
        () =>
        {
          Log.Information($"All checks executed...");
        });

    /// <summary>
    /// dotnet nuke --target CommitCheck
    /// </summary>
    protected virtual Target CommitCheck => _ => _
      .DependsOn(HuskyInstall)
      .OnlyWhenStatic(() => GitRepository != null)
      .Description("Run husky to check commit lint rules (if exists)")
      .Executes(
        () =>
        {
          var dir = Solution.Directory / ".husky";
          Assert.True(dir.DirectoryExists(), message: ".husky/ does not exist");

          DotNet(string.Join(' ', DefaultHuskyArgs), Repository.RootDirectory);
        });

    /// <summary>
    /// dotnet nuke --target BetaCheck
    /// Checks the solution for beta-version packages, using the package filter
    /// </summary>
    public virtual Target BetaCheck => _ => _
      .Description("Run dotnet outdated to check for beta packages")
      .DependsOn(BuildDependencyTree)
      .Executes(
        () =>
        {
          PackageManager.CheckBetaPackages();
        });

    /// <summary>
    /// dotnet nuke --target OutdatedCheck
    /// Checks the solution for outdated packages, using the package filter
    /// </summary>
    internal virtual Target OutdatedCheck => _ => _
      .Description(
        "Run dotnet outdated to check for outdated packages using --packages-filter defaulted to internal packages")
      .DependsOn(BuildDependencyTree)
      .Executes(
        () =>
        {
          PackageManager.CheckOutdatedPackages();
        });

    /// <summary>
    /// dotnet nuke --target VulnerableScan
    /// Runs dotnet list package --vulnerable and reports results via telemetry
    /// </summary>
    protected virtual Target VulnerabilityCheck => _ => _
      .Description(
        "Performs 'dotnet list package --vulnerable' and reports results via telemetry")
      .Before(RunChecks)
      .Executes(
        () =>
        {
          Log.Information($"Scanning dependencies for vulnerabilities ...");

          VulnerabilityScanner.Scan(this);
        });
  }
}