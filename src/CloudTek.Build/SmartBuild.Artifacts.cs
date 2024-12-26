using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using CloudTek.Build.Extensions;
using CloudTek.Build.Primitives;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using Serilog;
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
    /// Flag indicating whether the dotnet publish products should be created in parallel. May cause concurrent file access issues.
    /// </summary>
    [Parameter("Publish artifacts in parallel")]
    public virtual bool PublishInParallel { get; set; }

    /// <summary>
    /// dotnet nuke --target PublishTests
    /// </summary>
    protected virtual Target PublishTests => _ => _
      .Description("Publish test artifacts to respective /artifacts/tests subdirectory")
      .After(Test)
      .DependsOn(Compile)
      .Executes(
        () =>
        {
          var repositories = Repository.Projects.Where(p => p.Type == ProjectType.Test);
          if (PublishInParallel)
          {
            Parallel.ForEach(repositories, PublishInternal);
          }
          else
          {
            foreach (var project in repositories)
            {
              PublishInternal(project);
            }
          }
        });

    /// <summary>
    /// dotnet nuke --target Publish
    /// </summary>
    protected virtual Target PublishArtifacts => _ => _
      .Description("Publish artifacts to respective /artifacts/* subdirectories")
      .After(Test)
      .DependsOn(Compile)
      .Executes(
        () =>
        {
          var repositories = Repository.Projects.Where(p => p.Type == ProjectType.Service);
          if (PublishInParallel)
          {
            Parallel.ForEach(repositories, PublishInternal);
          }
          else
          {
            foreach (var project in repositories)
            {
              PublishInternal(project);
            }
          }
        });

    protected virtual Target Publish => _ => _
      .DependsOn(PublishTests, PublishArtifacts)
      .Executes(() =>
      {
        Log.Information($"All artifacts published...");
      });

    private void PublishInternal(Project project)
    {
      var output = project.Type switch
      {
        ProjectType.Test => Repository.ArtifactTestsDirectory,
        _ => Repository.ArtifactServicesDirectory
      };

      DotNetPublish(
        s => s
          .SetProject(project.Path)
          .Execute(settings => VersioningStrategy.SetDotNetPublishVersion(settings, this))
          .SetConfiguration(Configuration)
          .SetOutput(output / project.Name)
          .SetPublishReadyToRun(ReadyToRun)
          .SetNoRestore(SolutionRestored && string.IsNullOrEmpty(Runtime))
          .SetNoBuild(SolutionBuilt && string.IsNullOrEmpty(Runtime))
          .SetProcessToolPath(DotNetPath)
          .ExecuteWhen(!Runtime.IsNullOrEmpty(), settings => settings.SetRuntime(Runtime)));

      if (PublishAsZip)
      {
        (output / project.Name).ZipTo(
          archiveFile: Repository.ArtifactServicesDirectory / $"{project.Name}.zip",
          fileMode: FileMode.CreateNew,
          compressionLevel: CompressionLevel.Optimal);

        (output / project.Name).DeleteDirectory();
      }
    }
  }
}