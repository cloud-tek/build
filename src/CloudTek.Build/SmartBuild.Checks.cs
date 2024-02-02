using Nuke.Common;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build;

public abstract partial class SmartBuild : NukeBuild
{
  /// <summary>
  ///   dotnet nuke --target Restore --skip-beta-check true
  /// </summary>
  [Parameter]
  public bool SkipBetaCheck { get; set; } = true;

  /// <summary>
  ///   dotnet nuke --target Restore --skip-outdated-check
  /// </summary>
  [Parameter]
  public bool SkipOutdatedCheck { get; set; } = true;

  /// <summary>
  ///   dotnet nuke --target Restore --skip-commit-check
  /// </summary>
  [Parameter]
  public bool SkipCommitCheck { get; set; }

  /// <summary>
  /// Installs Husky.NET
  /// </summary>
  protected virtual Target HuskyInstall => _ => _
    .OnlyWhenDynamic(() => !SkipCommitCheck)
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
    .DependsOn(CommitLintCheck, FormatCheck, PackagesBetaCheck, PackagesOutdatedCheck)
    .WhenSkipped(DependencyBehavior.Skip)
    .Executes(() => { Log.Information("All checks executed..."); });

  /// <summary>
  /// dotnet nuke --target CommitLintCheck
  /// Executes Husky.NET to check the commit message
  /// </summary>
  protected virtual Target CommitLintCheck => _ => _
    .DependsOn(HuskyInstall)
    .OnlyWhenDynamic(() => !SkipCommitCheck)
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
  protected virtual Target PackagesBetaCheck => _ => _
    .OnlyWhenDynamic(() => !SkipBetaCheck)
    .DependsOn(BuildDependencyTree)
    .Executes(() => { PackageManager.CheckBetaPackages(this); });

  /// <summary>
  /// dotnet nuke --target PackagesOutdatedCheck
  /// Uses the PackageManager to check the solution for OUTDATED packages
  /// </summary>
  protected virtual Target PackagesOutdatedCheck => _ => _
    .OnlyWhenDynamic(() => !SkipOutdatedCheck)
    .DependsOn(BuildDependencyTree)
    .Executes(() => { PackageManager.CheckOutdatedPackages(this); });
}