using System.IO.Compression;
using CloudTek.Build.Extensions;
using CloudTek.Build.Primitives;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build
{
  public partial class SmartBuild
  {
    /// <summary>
    /// The target runtime to be used for dotnet publish
    /// </summary>
    [Parameter("Runtime for dotnet commands. Defaults to an empty string (ignored during dotnet publish).")]
    public virtual string Runtime { get; set; } = string.Empty;

    /// <summary>
    /// Ready to Run flag, to enable faster startups and reduce JIT'ting
    /// </summary>
    [Parameter("Ready to Run for dotnet commands. Default is false")]
    public virtual bool ReadyToRun { get; set; }

    /// <summary>
    /// Flag indicating whether the dotnet publish products should be published as artifacts
    /// </summary>
    [Parameter("Publish artifact(s) as .zip file(s)")]
    public virtual bool PublishAsZip { get; set; }

    /// <summary>
    /// Flag indicating whether the dotnet publish products should include test assemblies
    /// </summary>
    [Parameter("Publish test assemblies as artifacts")]
    public virtual bool PublishTests { get; set; }

    /// <summary>
    /// Flag indicating whether the dotnet publish products should be created in parallel. May cause concurrent file access issues.
    /// </summary>
    [Parameter("Publish artifacts in parallel")]
    public virtual bool PublishInParallel { get; set; }

    /// <summary>
    /// dotnet nuke --target Publish
    /// </summary>
    protected virtual Target Publish => _ => _
      .Description("Publish artifacts to respective /artifacts/* subdirectories")
      .After(Test)
      .DependsOn(Compile)
      .Executes(
        () =>
        {
          if (PublishInParallel)
          {
            Parallel.ForEach(
              Repository.Projects.Where(p => p.Type == ProjectType.Service || (PublishTests && p.Type == ProjectType.Test)),
              project =>
              {
                PublishInternal(project);
              });
          }
          else
          {
            foreach (var project in Repository.Projects.Where(p => p.Type == ProjectType.Service || (PublishTests && p.Type == ProjectType.Test)))
            {
              PublishInternal(project);
            }
          }
        });

    private void PublishInternal(Project project)
    {
      var output = project.Type switch
      {
        ProjectType.Test => Repository.ArtifactTestsDirectory / project.Name,
        _ => Repository.ArtifactServicesDirectory / project.Name
      };

      DotNetPublish(
        s => s
          .SetProject(project.Path)
          .Execute(settings => VersioningStrategy.SetDotNetPublishVersion(settings, this))
          .SetConfiguration(Configuration)
          .SetOutput(output)
          .SetPublishReadyToRun(ReadyToRun)
          .SetRuntime(Runtime)
          .SetNoRestore(SolutionRestored)
          .SetNoBuild(SolutionBuilt)
          .SetProcessToolPath(DotNetPath));

      if (PublishAsZip)
      {
        output.ZipTo(
          archiveFile: Repository.ArtifactServicesDirectory / $"{project.Name}.zip",
          fileMode: FileMode.CreateNew,
          compressionLevel: CompressionLevel.Optimal);

        output.DeleteDirectory();
      }
    }
  }
}