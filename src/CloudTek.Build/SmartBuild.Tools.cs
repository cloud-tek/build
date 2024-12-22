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
              .SetProcessAdditionalArguments("--create-manifest-if-needed"));
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

          try
          {
            if (!path.DirectoryExists())
            {
              await Assembly.GetExecutingAssembly().CopyResources(path, ".templates..husky");

              DotNetToolInstall(
                c =>
                  c.SetPackageName("husky")
                    .SetProcessAdditionalArguments("--create-manifest-if-needed"));

              Log.Information("husky installed");
            }
            else
            {
              Log.Information("husky already installed");
            }
          }
          finally
          {
            DotNetToolRestore();
            DotNet("husky install", RootDirectory);
          }
        });
  }
}