using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build
{
  public abstract partial class SmartBuild
  {
    /// <summary>
    /// dotnet nuke --target Clean
    /// </summary>
    protected virtual Target Clean => _ => _
      .Description("Clean build outputs and artifacts")
      .Executes(
        () =>
        {
          Repository.ArtifactsDirectory.CreateOrCleanDirectory();
          Repository.TestResultsDirectory.CreateOrCleanDirectory();
          Repository.TestCoverageDirectory.CreateOrCleanDirectory();

          Repository
            .Projects.ForEach(
              project =>
              {
                AbsolutePath.Create(project.WorkDir)
                  .GlobDirectories("{obj,bin}")
                  .DeleteDirectories();
              });
        });

    /// <summary>
    /// dotnet nuke --target Compile
    /// </summary>
    protected virtual Target Compile => _ => _
      .DependsOn(Restore)
      .Executes(
        () =>
        {
          DotNetBuild(
            s => s
              .SetProcessWorkingDirectory(Solution.Directory)
              .SetConfiguration(Configuration)
              .SetNoRestore(SolutionRestored)
              .SetProcessToolPath(DotNetPath));

          SolutionBuilt = true;
        });
  }
}