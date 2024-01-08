using CloudTek.Build.Primitives;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build;

public abstract partial class SmartBuild : NukeBuild
{
  private bool _solutionBuilt;

  /// <summary>
  /// Parameter indicating that the solution is not to be built
  /// </summary>
  [Parameter] public bool NoBuild { get; set; }

  /// <summary>
  /// Flag indicating that the solution has been built during this execution of SmartBuild
  /// </summary>
  public bool SolutionBuilt
  {
    get => _solutionBuilt || NoBuild;

    private set => _solutionBuilt = value;
  }

  /// <summary>
  /// dotnet nuke --target Initialize
  /// Required by all targets to properly initialize the SmartBuild
  /// </summary>
  protected virtual Target Initialize => _ => _
    .Executes(() =>
    {
      Repository
        .Artifacts
        .ForEach(artifact => { artifact.Initialize(); });

      Repository.DetectTests(this);

      Log.Information("IsLocalBuild: {IsLocalBuild}", IsLocalBuild);
      Log.Information("SkipCommitCheck: {SkipCommitCheck}", SkipCommitCheck);
      Log.Information("SkipBetaCheck: {SkipBetaCheck}", SkipBetaCheck);
      Log.Information("SkipOutdatedCheck: {SkipOutdatedCheck}", SkipOutdatedCheck);
      Log.Information("SkipFormatCheck: {SkipFormatCheck}", SkipFormatCheck);
    });

  /// <summary>
  /// dotnet nuke --target Clean
  /// </summary>
  protected virtual Target Clean => _ => _
    .DependsOn(Initialize)
    .Executes(() =>
    {
      Repository.ArtifactsDirectory.CreateOrCleanDirectory();
      Repository.TestResultsDirectory.CreateOrCleanDirectory();
      Repository.TestCoverageDirectory.CreateOrCleanDirectory();

      Solution
        .Projects
        .ForEach(project =>
        {
          project.Path
              .GlobDirectories("{obj,bin}")
              .DeleteDirectories();
        });
    });

  /// <summary>
  /// dotnet nuke --target Compile
  /// </summary>
  protected virtual Target Compile => _ => _
    .DependsOn(Initialize, Restore, RunChecks)
    .Executes(() =>
    {
      DotNetBuild(s => s
        .SetProcessWorkingDirectory(Solution.Directory)
        .SetConfiguration(Configuration)
        .SetNoRestore(SolutionRestored)
        .SetProcessToolPath(DotNetPath));

      SolutionBuilt = true;
    });

  /// <summary>
  /// dotnet nuke --target Publish
  /// </summary>
  protected virtual Target Publish => _ => _
    .DependsOn(Restore)
    .Executes(() =>
    {
      Repository.Artifacts.Where(a => a.Type == ArtifactType.Service || a.Type == ArtifactType.Demo).ForEach(artifact =>
      {
        artifact.Projects.ForEach(project =>
        {
          DotNetPublish(s => s
            .SetProject(project.Path)
            .SetConfiguration(Configuration)
            .SetOutput(Repository.ServicesDirectory / project.Name)
            .SetNoRestore(SolutionRestored)
            .SetNoBuild(SolutionBuilt)
            .SetProcessToolPath(DotNetPath));
        });
      });
    });
}