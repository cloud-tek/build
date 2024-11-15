using System.Text;
using System.Text.RegularExpressions;
using CloudTek.Build.Extensions;
using CloudTek.Build.Versioning;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build.Packaging;

/// <summary>
/// The default NuGet package manager
/// </summary>
public sealed class PackageManager
{
  private static readonly Regex ProjectRegex = new("(») (.*)");
  private static readonly Regex BetaRegex = new(" \\d+.\\d+.\\d+-", RegexOptions.Compiled);
  private static readonly Regex OutdatedRegex = new("-> \\S", RegexOptions.Compiled);

  private static readonly MsBuildFileHandler MsbuildFileHandler = new();
  private readonly List<string> _betaOutputs = new List<string>();
  private readonly List<string> _outdatedOutputs = new List<string>();
  private readonly Dictionary<string, ISet<string>> _pinnedPackages = new();

  /// <summary>
  /// Restores packages in the solution
  /// </summary>
  /// <param name="build"></param>
#pragma warning disable CA1822
  public void Restore(SmartBuild build)
#pragma warning restore CA1822
  {
    Log.Debug("Restoring packages in the solution ...");
    DotNetRestore(
      x => x
        .SetProcessWorkingDirectory(build.Solution.Directory)
        .SetProcessToolPath(DotNetPath)
        .SetRuntime(build.Runtime)
        .SetForceEvaluate(build.ReadyToRun)
    );
  }

  /// <summary>
  /// Retrieves the set of packages pinned in projects, and globally in the Directory.Packages.props when CPM is used
  /// </summary>
  /// <param name="build"></param>
  private void GetPinnedPackages(SmartBuild build)
  {
    foreach (var project in build.Solution.AllProjects)
    {
      _pinnedPackages[project] = MsbuildFileHandler.GetPinnedPackageReference(project.Path).ToHashSet();
    }

    var path = build.Solution.Directory / "Directory.Packages.props";
    if (path.FileExists())
    {
      _pinnedPackages["global"] = MsbuildFileHandler.GetPinnedPackageVersion(path).ToHashSet();
    }
  }

  /// <summary>
  /// Pack packages into nuget packages
  /// </summary>
  /// <param name="repository"></param>
  /// <param name="build"></param>
  /// <param name="strategy"></param>
  /// <param name="configuration"></param>
#pragma warning disable CA1822
  public void Pack(
#pragma warning restore CA1822
    Repository repository,
    SmartBuild build,
    VersioningStrategy strategy,
    Configuration configuration)
  {
    DotNetPack(
      s => s
        .SetConfiguration(configuration)
        .Execute(settings => strategy.SetDotNetPackVersion(settings, build))
        .SetNoRestore(build.SolutionRestored)
        .SetNoBuild(build.SolutionBuilt)
        .SetOutputDirectory(repository.ArtifactPackagesDirectory)
        .SetProcessToolPath(DotNetPath));
  }

  /// <summary>
  /// Build the dependency tree required for the checks
  /// </summary>
  /// <param name="build"></param>
  /// <exception cref="NotSupportedException">Thrown when an unsupported parsing mode is requested</exception>
  public void BuildDependencyTree(SmartBuild build)
  {
    GetPinnedPackages(build);
    var outputs = RunOutdatedCommand(build);
    var project = string.Empty;

    foreach (var output in outputs)
    {
      project = GetProject(output, project);

      var isBetaMatch = IsBetaMatch(output, project);
      var isOutdatedMatch = OutdatedRegex.IsMatch(output.Text);

      if (isBetaMatch)
        _betaOutputs.Add(output.Text);
      if (isOutdatedMatch)
        _outdatedOutputs.Add(output.Text);
    }
  }

  private bool IsBetaMatch(Output output, string project)
  {
    var isBetaMatch = BetaRegex.IsMatch(output.Text);

    if (isBetaMatch)
    {
      if (_pinnedPackages.GetValueOrDefault("global")?.Any(p => output.Text.Contains(p)) ?? false)
      {
        isBetaMatch = false;
      }

      if (_pinnedPackages.GetValueOrDefault(project)?.Any(p => output.Text.Contains(p)) ?? false)
      {
        isBetaMatch = false;
      }
    }

    return isBetaMatch;
  }

  private static string GetProject(Output output, string project)
  {
    var match = ProjectRegex.Match(output.Text);
    if (match.Success)
    {
      project = match.Groups[2].Value;
    }

    return project;
  }

  private static IReadOnlyCollection<Output> RunOutdatedCommand(SmartBuild build)
  {
    var command = "outdated --include-up-to-date";

    foreach (var packageInclusion in build.PackagesFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries))
    {
      command += " --include " + packageInclusion;
    }

    var outputs = DotNet(
      command,
      workingDirectory: build.Solution.Directory);
    return outputs;
  }

  /// <summary>
  /// Checks if the solution's outputs contain any BETA packages
  /// </summary>
  public void CheckBetaPackages()
  {
    if (_betaOutputs.Any())
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("BETA packages detected!");
      foreach (var betaOutput in _betaOutputs)
      {
        stringBuilder.AppendLine(betaOutput);
      }

      Assert.Fail(stringBuilder.ToString());
    }
    else
    {
      Log.Information("No beta packages detected");
    }
  }

  /// <summary>
  /// Checks if the solution's outputs contain any BETA packages
  /// </summary>
  public void CheckOutdatedPackages()
  {
    if (_outdatedOutputs.Any())
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.AppendLine("Outdated packages detected!");
      foreach (var outdatedOutput in _outdatedOutputs)
      {
        stringBuilder.AppendLine(outdatedOutput);
      }

      Assert.Fail(stringBuilder.ToString());
    }
    else
    {
      Log.Information("No outdated packages detected");
    }
  }
}