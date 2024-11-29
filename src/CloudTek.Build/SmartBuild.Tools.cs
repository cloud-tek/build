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
          var path = RootDirectory / ".husky";

          if (!path.Exists())
          {
            await Assembly.GetExecutingAssembly().CopyResources(path, ".templates..husky");

            DotNetToolInstall(
              c =>
                c.SetPackageName("husky")
                  .SetProcessArgumentConfigurator(p => p.Add("--create-manifest-if-needed")));

            DotNetToolRestore();
            DotNet("husky install", RootDirectory);

            Log.Information("husky installed");
          }
          else
          {
            Log.Information("husky already installed");
          }
        });
  }
}