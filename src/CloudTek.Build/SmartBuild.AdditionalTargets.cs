using Nuke.Common;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build;

public abstract partial class SmartBuild
{
  /// <summary>
  /// dotnet nuke --target UpdateOutdated
  /// Runs dotnet outdated to update packages with --packages-filter defaulted to packages matching the package filter
  /// </summary>
  protected virtual Target UpdateOutdated => _ => _
    .Description("Run dotnet outdated to update packages with --packages-filter defaulted to packages matching the package filter")
    .DependsOn(DotnetOutdatedInstall)
    .Executes(
      () =>
      {
        var command = "outdated -u";
        foreach (var include in PackagesFilter.Split(
                   " ",
                   StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
          command += " --include " + include;
        }

        DotNet(command, Solution.Directory);
        PackageManager.CheckOutdatedPackages();
      });
}