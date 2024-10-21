using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning;

public abstract partial class VersioningStrategy
{
  /// <summary>
  /// Versioning strategy utilizing https://gitversion.net/
  /// </summary>
  public class GitVersion : VersioningStrategy
  {
    internal override Func<DotNetPackSettings, SmartBuild, DotNetPackSettings> SetDotNetPackVersion
    {
      get;
    }
      = (settings, build) =>
      {
        var gv = GetGitVersion(build);

        return settings
          .SetVersion(gv.NuGetVersionV2)
          .SetFileVersion(gv.AssemblySemFileVer)
          .SetAssemblyVersion(gv.AssemblySemVer);
      };

    private static Nuke.Common.Tools.GitVersion.GitVersion GetGitVersion(SmartBuild build)
    {
      var t = build.GetType();

      var propInfo = t.GetProperty("GitVersion")
                     ?? throw new InvalidOperationException(
                       "SmartBuild does not contain a GitVersion property");

      var result = propInfo.GetValue(build) as Nuke.Common.Tools.GitVersion.GitVersion;

      return result ?? throw new InvalidOperationException(
        "GitVersion property does not contain a valid Nuke.Common.Tools.GitVersion.GitVersion object");
    }
  }
}