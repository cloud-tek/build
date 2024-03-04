using Nuke.Common;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build;

public abstract partial class SmartBuild : NukeBuild
{
  /// <summary>
  /// Installs Husky.NET
  /// </summary>
  protected virtual Target HuskyInstall => _ => _
    .BaseTarget(nameof(HuskyInstall), this)
    .Executes(() =>
    {
      DotNet(string.Join(' ', HuskyInstallArgs), Solution.Directory);
    });

  private static readonly string[] HuskyInstallArgs = new[]
  {
    "husky",
    "install"
  };
  private static readonly string[] HuskyArgs = new[]
  {
    "husky",
    "run",
    "--name",
    "commit-message-linter-ci",
    "--args",
    "origin/main.."
  };

  /// <summary>
  /// dotnet nuke --target RunChecks
  /// A meta-target aggregating all pre-build checks
  /// </summary>
  protected virtual Target RunChecks => _ => _
    .BaseTarget(nameof(RunChecks), this)
    .DependsOn(CommitLintCheck, FormatCheck, PackagesBetaCheck, PackagesOutdatedCheck)
    .Before(UnitTests)
    .WhenSkipped(DependencyBehavior.Skip)
    .Executes(() => { Log.Information("All checks executed..."); });

  /// <summary>
  /// dotnet nuke --target CommitLintCheck
  /// Executes Husky.NET to check the commit message
  /// </summary>
  protected virtual Target CommitLintCheck => _ => _
    .BaseTarget(nameof(CommitLintCheck), this)
    .DependsOn(HuskyInstall)
    .Executes(() =>
    {
      var huskyDir = $"{Solution.Directory}/.husky";
      Assert.True(Directory.Exists(huskyDir), ".husky/ does not exist");

      DotNet(string.Join(' ', HuskyArgs), Solution.Directory);
    });

  /// <summary>
  /// dotnet nuke --target PackagesBetaCheck
  /// Uses the PackageManager to check the solution for BETA packages
  /// </summary>
  protected internal virtual Target PackagesBetaCheck => _ => _
    .BaseTarget(nameof(PackagesBetaCheck), this)
    .DependsOn(BuildDependencyTree)
    .Executes(() => { PackageManager.CheckBetaPackages(this); });

  /// <summary>
  /// dotnet nuke --target PackagesOutdatedCheck
  /// Uses the PackageManager to check the solution for OUTDATED packages
  /// </summary>
  protected internal virtual Target PackagesOutdatedCheck => _ => _
    .BaseTarget(nameof(PackagesOutdatedCheck), this)
    .DependsOn(BuildDependencyTree)
    .Executes(() => { PackageManager.CheckOutdatedPackages(this); });
}