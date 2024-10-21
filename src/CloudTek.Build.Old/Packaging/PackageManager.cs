using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build.Packaging;

/// <summary>
///   Due to the fact that 'dotnet list package' behaves differently when --outdated or --vulnerable flags are passed,
///   we need alternate parse modes
/// </summary>
internal enum PackageParsingMode
{
  Default, // Used for beta version detection
  Outdated // Used for latest version detection
}

/// <summary>
/// The package manager abstraction
/// </summary>
public abstract partial class PackageManager
{
#pragma warning disable MA0009
  private static readonly Regex PackageBetaVersionRegex = new(@"^(\S*)(-)(\S*)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
#pragma warning restore MA0009
  private static readonly MSBuildFileHandler MsbuildFileHandler = new();

  /// <summary>
  /// Projects' package sets
  /// </summary>
  protected IDictionary<string, ISet<Package>> Packages { get; init; }
    = new Dictionary<string, ISet<Package>>();

  /// <summary>
  /// A set of pinned packages in the solution
  /// </summary>
  protected IDictionary<string, ISet<string>> PinnedPackages { get; init; } =
    new Dictionary<string, ISet<string>>();

  internal abstract ISet<string> GetPinnedPackages(SmartBuild build, string project);

  /// <summary>
  ///   Restore packages for all artifacts in the repository
  /// </summary>
  /// <param name="build"></param>
  public abstract void Restore(SmartBuild build);

  /// <summary>
  ///   Pack packages into nuget packages
  /// </summary>
  /// <param name="repository"></param>
  /// <param name="build"></param>
  /// <param name="strategy"></param>
  /// <param name="configuration"></param>
  public virtual void Pack(
    Repository repository,
    SmartBuild build,
    VersioningStrategy strategy,
    Configuration configuration)
  {
    repository.Artifacts.Where(a => a.Type == ArtifactType.Package).ForEach(artifact =>
    {
      artifact.Projects.Where(p => p.IsPackable).ForEach(project =>
      {
        DotNetPack(s => s
          .SetProject(project.Path)
          .SetConfiguration(configuration)
          .Execute(settings => strategy.SetDotNetPackVersion(settings, build, project))
          .SetNoRestore(build.SolutionRestored)
          .SetOutputDirectory(repository.PackagesDirectory / project.Name)
          .SetProcessToolPath(DotNetPath));
      });
    });
  }

  /// <summary>
  ///   Push packages emitted by all package artifacts in the repository
  /// </summary>
  /// <param name="repository"></param>
  /// <param name="build"></param>
  /// <param name="strategy"></param>
  /// <param name="nugetApiUrl"></param>
  /// <param name="nugetApiKey"></param>
  public virtual void Push(
    Repository repository,
    SmartBuild build,
    VersioningStrategy strategy,
    string nugetApiUrl,
    string nugetApiKey)
  {
    repository.Artifacts.Where(a => a.Type == ArtifactType.Package).ForEach(artifact =>
    {
      artifact.Projects.Where(p => p.IsPackable).ForEach(project =>
      {
        DotNetNuGetPush(s => s
          .Execute(settings => strategy.SetDotNetNuPkgPath(settings, build, project))
          .SetSource(nugetApiUrl)
          .SetApiKey(nugetApiKey)
          .SetSymbolSource(nugetApiUrl)
          .SetSymbolApiKey(nugetApiKey)
          .SetNoSymbols(false)
          .SetSkipDuplicate(true)
          .SetProcessToolPath(DotNetPath)
        );
      });
    });
  }

  /// <summary>
  /// Checks for beta packages matching the PackageChecksRegex
  /// </summary>
  /// <param name="build"></param>
  public void CheckBetaPackages(SmartBuild build)
  {
    var result = false;
    foreach (var kvp in Packages)
    {
      var packages = kvp.Value
        .Where(p => build.PackageChecksRegex.IsMatch(p.Name))
        .Where(p => PackageBetaVersionRegex.IsMatch(p.Resolved))
        .Where(p => !PinnedPackages[kvp.Key].Contains(p.Name));

      if (packages.Any())
      {
        result = true;
        var sb = new StringBuilder($"{kvp.Key}: following (unpinned) BETA packages found: ");
        packages.ForEach(p => sb.Append(CultureInfo.InvariantCulture, $" {p.Name} ({p.Resolved}) "));

        Log.Logger.Error(sb.ToString().TrimEnd());
      }
      else
      {
        Log.Logger.Debug($"{kvp.Key}: no (unpinned) BETA packages found");
      }
    }

    if (result)
      Assert.Fail("At least one project contains BETA packages");
  }

  /// <summary>
  /// Checks for oudated packages matching the PackageChecksRegex
  /// </summary>
  /// <param name="build"></param>
  public void CheckOutdatedPackages(SmartBuild build)
  {
    var result = false;
    foreach (var kvp in Packages)
    {
      var packages = kvp.Value
        .Where(p => p.Resolved != p.Latest)
        .Where(p => build.PackageChecksRegex.IsMatch(p.Name))
        .Where(p => !PinnedPackages[kvp.Key].Contains(p.Name));

      if (packages.Any())
      {
        result = true;
        var sb = new StringBuilder($"{kvp.Key}: following (unpinned) OUTDATED packages found: ");
        packages.ForEach(p => sb.Append(CultureInfo.InvariantCulture, $" {p.Name} ({p.Resolved}) "));

        Log.Logger.Error(sb.ToString().TrimEnd());
      }
      else
      {
        Log.Logger.Information($"{kvp.Key}: no (unpinned) OUTDATED packages found");
      }
    }

    if (result)
      Assert.Fail("At least one project contains OUTDATED packages");
  }
}