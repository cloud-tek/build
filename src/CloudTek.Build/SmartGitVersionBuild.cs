using CloudTek.Build.Versioning;
using Nuke.Common.Tools.GitVersion;

namespace CloudTek.Build
{
  /// <summary>
  /// A SmartBuild using GitVersion for versioning
  /// </summary>
  public abstract class SmartGitVersionBuild : SmartBuild<VersioningStrategy.GitVersion>
  {
    /// <summary>
    /// GitVersion information for the SmartBuild
    /// </summary>
    [GitVersion(Framework = "net5.0", NoFetch = true)]
    public GitVersion GitVersion { get; set; } = default!;
  }
}