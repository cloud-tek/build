using Nuke.Common;
using Serilog;

namespace CloudTek.Build;

public partial class SmartBuild
{
  private bool _solutionRestored;

  /// <summary>
  /// Parameter indicating that the solution is not to be restored
  /// </summary>
  [Parameter] public bool NoRestore { get; set; }

  /// <summary>
  /// Flag indicating whether the solution has been restored during this execution of SmartBuild
  /// </summary>
  public bool SolutionRestored
  {
    get => _solutionRestored || NoRestore;
    private set => _solutionRestored = value;
  }

  /// <summary>
  /// dotnet nuke --target Restore
  /// Restores the packages for the solution using the PackageManager
  /// </summary>
  protected internal virtual Target Restore => _ => _
    .DependsOn(Clean)
    .WhenSkipped(DependencyBehavior.Execute)
    .Executes(() =>
    {
      Log.Information("Restoring packages...");
      PackageManager.Restore(this);

      SolutionRestored = true;
    });

  /// <summary>
  /// dotnet nuke --target Pack
  /// Packs the artifacts with the type equal to 'Package'
  /// </summary>
  protected internal virtual Target Pack => _ => _
    .CheckIfSkipped(nameof(Pack), this)
    .WhenSkipped(DependencyBehavior.Execute)
    .DependsOn(Test)
    .Executes(() =>
    {
      Log.Information("Packing NuGet packages...");

      PackageManager.Pack(Repository, this, VersioningStrategy, Configuration);
    });

  /// <summary>
  /// dotnet nuke --target Push
  /// Pushes the package artifacts to the NuGet feed
  /// </summary>
  protected internal virtual Target Push => _ => _
    .CheckIfSkipped(nameof(Push), this)
    .Requires(() => NugetApiUrl)
    .Requires(() => NugetApiKey)
    .Executes(() =>
    {
      Log.Information($"Pushing to: {NugetApiUrl}");

      PackageManager.Push(Repository, this, VersioningStrategy, NugetApiUrl, NugetApiKey);
    });

  /// <summary>
  /// dotnet nuke --target BuildDependencyTree
  /// Determines the dependencies of the soltion for BETA/Outdated/Vulnerability checks
  /// </summary>
  protected internal virtual Target BuildDependencyTree => _ => _
    .CheckIfSkipped(nameof(BuildDependencyTree), this)
    .DependsOn(Restore)
    .Executes(() => { PackageManager.BuildDependencyTree(this); });
}