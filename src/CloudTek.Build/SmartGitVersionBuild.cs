using CloudTek.Build.Packaging;
using CloudTek.Build.Versioning;
using Nuke.Common.Tools.GitVersion;

namespace CloudTek.Build;

/// <summary>
/// Abstract SmartBuild supporting GitVersion
/// </summary>
/// <typeparam name="TPackageManager">Type of package manager</typeparam>
public abstract class SmartGitVersionBuild<TPackageManager> : SmartBuild<TPackageManager, VersioningStrategy.GitVersion>
  where TPackageManager : PackageManager, new()
{
  /// <summary>
  /// Contructor for SmartGitVersionBuild
  /// </summary>
  /// <param name="repository"></param>
  protected SmartGitVersionBuild(Repository repository) : base(repository)
  {
  }

  /// <summary>
  /// GitVersion information for SmartBuild
  /// </summary>
  [GitVersion(Framework = "net5.0", NoFetch = true)]
  public GitVersion GitVersion { get; set; } = default!;
}