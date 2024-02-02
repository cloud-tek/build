using CloudTek.Build.Primitives;
using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning;

/// <summary>
/// Abstract versioning strategy for SmartBuild
/// </summary>
public abstract partial class VersioningStrategy
{
  /// <summary>
  /// GitVersion versioning strategy for SmartBuild
  /// </summary>
  public class GitVersion : VersioningStrategy
  {
    internal override Func<DotNetPublishSettings, SmartBuild, Project, DotNetPublishSettings>
      SetDotNetPublishVersion
    { get; } = (settings, build, project) =>
    {
      var gv = GetGitVersion(build);

      return settings
        .SetVersion(gv.NuGetVersionV2)
        .SetFileVersion(gv.AssemblySemFileVer)
        .SetAssemblyVersion(gv.AssemblySemVer);
    };

    internal override Func<DotNetPackSettings, SmartBuild, Project, DotNetPackSettings> SetDotNetPackVersion { get; }
      = (settings, build, project) =>
      {
        var gv = GetGitVersion(build);

        return settings
          .SetVersion(gv.NuGetVersionV2)
          .SetFileVersion(gv.AssemblySemFileVer)
          .SetAssemblyVersion(gv.AssemblySemVer);
      };

    internal override Func<DotNetNuGetPushSettings, SmartBuild, Project, DotNetNuGetPushSettings>
      SetDotNetNuPkgPath
    { get; } = (settings, build, project) => GetPushSettings(settings, build, project, "nupkg");

    private static DotNetNuGetPushSettings GetPushSettings(
      DotNetNuGetPushSettings settings,
      SmartBuild build,
      Project project,
      string format)
    {
      var gv = GetGitVersion(build);

      return settings
        .SetTargetPath(build.Repository.PackagesDirectory / project.Name /
                       $"{project.Name}.{gv.NuGetVersionV2}.{format}");
    }

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