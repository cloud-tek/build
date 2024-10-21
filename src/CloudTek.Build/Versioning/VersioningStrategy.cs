using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning
{
  /// <summary>
  /// An abstract class used to feed dotnet pack with version information for packages
  /// </summary>
  public abstract partial class VersioningStrategy
  {
    internal abstract Func<DotNetPackSettings, SmartBuild, DotNetPackSettings> SetDotNetPackVersion { get; }
  }
}