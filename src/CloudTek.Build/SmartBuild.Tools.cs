using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CloudTek.Build.Internals;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build
{
  public abstract partial class SmartBuild
  {
    /// <summary>
    /// dotnet nuke --target DotnetOutdatedInstall
    /// Restores dotnet-outdated-tool from the local tool manifest.
    /// Installs it only if the manifest does not list it; never upgrades a pinned version.
    /// </summary>
#pragma warning disable CA1822
    public virtual Target DotnetOutdatedInstall => _ => _
      .Executes(
        () =>
        {
          EnsureLocalTool("dotnet-outdated-tool");
          DotNetToolRestore();
        });
#pragma warning restore CA1822

    /// <summary>
    /// dotnet nuke --target HuskyInstall
    /// Restores Husky.NET from the local tool manifest.
    /// Installs it only if the manifest does not list it; never upgrades a pinned version.
    /// </summary>
    protected virtual Target HuskyInstall => _ => _
      .Executes(
        async () =>
        {
          var path = RootDirectory / ".husky";

          try
          {
            if (!path.DirectoryExists())
            {
              await Assembly.GetExecutingAssembly().CopyResources(path, ".templates..husky");

              Log.Information("husky hook templates copied");
            }
            else
            {
              Log.Information("husky hooks already present");
            }

            EnsureLocalTool("husky");
          }
          finally
          {
            DotNetToolRestore();
            DotNet("husky install", RootDirectory);
          }
        });

    private static void EnsureLocalTool(string packageId)
    {
      if (ToolManifest.ContainsTool(RootDirectory, packageId))
      {
        Log.Information("{Tool} is listed in the tool manifest; restoring the pinned version", packageId);
        return;
      }

      Log.Information("{Tool} is missing from the tool manifest; installing it", packageId);
      DotNetToolInstall(
        c => c.SetPackageName(packageId)
          .SetProcessAdditionalArguments("--create-manifest-if-needed"));
    }
  }
}