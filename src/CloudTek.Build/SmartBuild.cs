using System.Collections;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CloudTek.Build.Packaging;
using CloudTek.Build.Versioning;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;

namespace CloudTek.Build;

/// <summary>
/// Abstract SmartBuild
/// </summary>
/// <typeparam name="TPackageManager">Type of package manager</typeparam>
/// <typeparam name="TVersioningStrategy">Type of versioning strategy</typeparam>
public abstract class SmartBuild<TPackageManager, TVersioningStrategy> : SmartBuild
  where TPackageManager : PackageManager, new()
  where TVersioningStrategy : VersioningStrategy, new()
{
  /// <summary>
  /// Contructor for SmartBuild
  /// </summary>
  /// <param name="repository"></param>
  protected SmartBuild(Repository repository)
    : base(repository, new TPackageManager(), new TVersioningStrategy())
  {
  }

  /// <summary>
  /// Contructor for SmartBuild
  /// </summary>
  /// <param name="repository"></param>
  /// <param name="solutionProvider"></param>
  protected SmartBuild(Repository repository, Func<Solution> solutionProvider)
    : base(repository, new TPackageManager(), new TVersioningStrategy(), solutionProvider)
  { }
}

/// <summary>
/// Abstract SmartBuild
/// </summary>
public abstract partial class SmartBuild : NukeBuild
{
  /// <summary>
  /// Package manager
  /// </summary>
  protected readonly PackageManager PackageManager;

  /// <summary>
  /// Repository
  /// </summary>
  public readonly Repository Repository;

  /// <summary>
  /// Solution information for SmartBuild
  /// </summary>
  [Solution] public readonly Solution Solution = default!;

  /// <summary>
  /// Versioning strategy for SmartBuild
  /// </summary>
  public readonly VersioningStrategy VersioningStrategy;

  /// <summary>
  /// Constructor for SmartBuild
  /// </summary>
  /// <param name="repository"></param>
  /// <param name="packageManager"></param>
  /// <param name="versioningStrategy"></param>
  /// <exception cref="ArgumentNullException">Thrown when one of the parameters is null.</exception>
  /// <exception cref="SmartBuildException">Thrown when SmartBuild-specific error occurs</exception>
  protected SmartBuild(
    Repository repository,
    PackageManager packageManager,
    VersioningStrategy versioningStrategy)
  {
    _ = repository ?? throw new ArgumentNullException(nameof(repository));
    if (repository.Artifacts == null || repository.Artifacts.Length == 0)
      throw new SmartBuildException(SmartBuildError.NoArtifacts);

    Repository = repository;
    Repository.Initialize(RootDirectory);
    PackageManager = packageManager;
    VersioningStrategy = versioningStrategy;

    EnvironmentVariables = new ReadOnlyDictionary<string, string>(Environment.GetEnvironmentVariables()
      .OfType<DictionaryEntry>()
      .ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!));
  }

  /// <summary>
  /// Constructor for SmartBuild
  /// </summary>
  /// <param name="repository"></param>
  /// <param name="packageManager"></param>
  /// <param name="versioningStrategy"></param>
  /// <param name="solutionProvider"></param>
  /// <exception cref="ArgumentNullException">Thrown when one of the parameters is null.</exception>
  /// <exception cref="SmartBuildException">Thrown when SmartBuild-specific error occurs</exception>
  protected SmartBuild(
    Repository repository,
    PackageManager packageManager,
    VersioningStrategy versioningStrategy,
    Func<Solution> solutionProvider)
  : this(repository, packageManager, versioningStrategy)
  {
    Solution = solutionProvider() ?? throw new ArgumentNullException(nameof(solutionProvider));
  }

  /// <summary>
  /// Environment variables
  /// </summary>
  protected IReadOnlyDictionary<string, string> EnvironmentVariables { get; init; }

  /// <summary>
  /// Filter used for dotnet test
  /// </summary>
  protected virtual string TestFilter { get; init; } = "Flaky!=true";
#pragma warning disable MA0009

  /// <summary>
  /// Regex used to match packages used in pre-build checks
  /// </summary>
  public virtual Regex PackageChecksRegex { get; init; } = new("^(CloudTek|Hive)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
#pragma warning restore MA0009

  /// <summary>
  /// Build configuration
  /// </summary>
  [Parameter("Configuration to _build - Default is 'Debug' (local) or 'Release' (server)")]
  public Configuration Configuration { get; set; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  /// <summary>
  /// CI Build number
  /// </summary>
  [Parameter("BuildNumber")] public string BuildNumber { get; set; } = default!;

  /// <summary>
  /// API URL for NuGet
  /// </summary>
  [Parameter] public string NugetApiUrl { get; set; } = default!;

  /// <summary>
  /// API Key for NuGet
  /// </summary>
  [Parameter] public string NugetApiKey { get; set; } = default!;

  /// <summary>
  /// GitRepository information for SmartBuild
  /// </summary>
  [GitRepository] public GitRepository GitRepository { get; set; } = default!;
}