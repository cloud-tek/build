using CloudTek.Build.Primitives;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build
{
  public partial class SmartBuild
  {
    /// <summary>
    /// dotnet nuke --target Publish
    /// </summary>
    protected virtual Target Publish => _ => _
      .Description("Publish services (auto-detected) to artifacts/services directory")
      .After(Test)
      .DependsOn(Compile)
      .Executes(
        () =>
        {
          //it can't be run as single dotnet publish command, as we want only specific projects to publish
          Parallel.ForEach(
            Repository.Projects.Where(p => p.Type == ProjectType.Service),
            project =>
            {
              DotNetPublish(
                s => s
                  .SetProject(project.Path)
                  .SetConfiguration(Configuration)
                  .SetOutput(Repository.ServicesDirectory / project.Name)
                  .SetPublishReadyToRun(ReadyToRun)
                  .SetRuntime(Runtime)
                  .SetProcessToolPath(DotNetPath));
            });
        });
  }
}