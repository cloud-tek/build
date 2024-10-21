using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build
{
  public abstract partial class SmartBuild
  {
    /// <summary>
    /// dotnet nuke --target DotnetOutdatedInstall
    /// Installs dotnet tool: dotnet-outdated-tool
    /// Updates outdated packages
    /// </summary>
#pragma warning disable CA1822
    public virtual Target DotnetOutdatedInstall => _ => _
      .Executes(
        () =>
        {
          DotNetToolInstall(
            c => c.SetPackageName("dotnet-outdated-tool")
              .SetProcessArgumentConfigurator(p => p.Add("--create-manifest-if-needed")));
          DotNetToolRestore();
        });
#pragma warning restore CA1822

    /// <summary>
    /// dotnet nuke --target HuskyInstall
    /// Installs Husky.NET
    /// </summary>
    protected virtual Target HuskyInstall => _ => _
      .Executes(
        async () =>
        {
          var buildPath = RootDirectory / ".husky";

          if (await CopyResourcesIfFolderDoesntExist(buildPath, ".template..husky"))
          {
            DotNetToolInstall(
              c =>
                c.SetPackageName("husky")
                  .SetProcessArgumentConfigurator(p => p.Add("--create-manifest-if-needed")));
          }
          else
          {
            Log.Information("There is already a .husky directory, skipping setup");
          }

          DotNet("husky install", Solution.Directory);
          DotNetToolRestore();
        });
  }
}