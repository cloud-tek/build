using CloudTek.Build.Primitives;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning;

public abstract partial class VersioningStrategy
{
  /// <summary>
  /// Default versioning strategy, relying on MSBuild properties
  /// </summary>
  public class Default : VersioningStrategy
  {
    private const string BetaSuffix = "beta";

    internal override Func<DotNetPublishSettings, SmartBuild, Project, DotNetPublishSettings>
      SetDotNetPublishVersion
    { get; } =
      (settings, build, project) =>
        settings
          .When(
            project.Type == ArtifactType.Package &&
            build.Repository.ShouldEmitBetaPackage(build.GitRepository),
            settings => settings
              .SetVersionSuffix(
                GetBetaBuildSuffix(build.GitRepository, BetaSuffix, build.BuildNumber)));

    internal override Func<DotNetPackSettings, SmartBuild, Project, DotNetPackSettings> SetDotNetPackVersion { get; } =
      (settings, build, project) =>
        settings
          .When(
            project.Type == ArtifactType.Package &&
            build.Repository.ShouldEmitBetaPackage(build.GitRepository),
            settings => settings
              .SetVersionSuffix(
                GetBetaBuildSuffix(build.GitRepository, BetaSuffix, build.BuildNumber)));

    internal override Func<DotNetNuGetPushSettings, SmartBuild, Project, DotNetNuGetPushSettings>
      SetDotNetNuPkgPath
    { get; } =
      (settings, build, project) => GetPushSettings(settings, build, project, "nupkg");

    private static DotNetNuGetPushSettings GetPushSettings(
      DotNetNuGetPushSettings settings,
      SmartBuild build,
      Project project,
      string format)
    {
      return settings
        .When(
          project.Type == ArtifactType.Package && build.Repository.ShouldEmitPackage(build.GitRepository),
          sttngs => sttngs
            .SetTargetPath(build.Repository.PackagesDirectory / project.Name /
                           $"{project.Name}.*.{format}"))
        .When(
          project.Type == ArtifactType.Package &&
          build.Repository.ShouldEmitBetaPackage(build.GitRepository),
          sttngs => sttngs
            .SetTargetPath(build.Repository.PackagesDirectory / project.Name /
                           $"{project.Name}.*-{GetBetaBuildSuffix(build.GitRepository, BetaSuffix, build.BuildNumber)}.{format}"));
    }

    private static string GetBetaBuildSuffix(GitRepository git, string suffix, string buildNumber)
    {
      _ = suffix ?? throw new ArgumentNullException(nameof(suffix));

      if (string.IsNullOrEmpty(buildNumber) || git.IsOnDevelopBranch())
        return suffix;

      return $"{suffix}{CleanBuildNumber(git, buildNumber)}";
    }

    private static string CleanBuildNumber(GitRepository git, string buildNumber)
    {
      if (buildNumber.Contains("merge_"))
        return buildNumber.Replace("merge_", string.Empty);
      return buildNumber.Split("_").Last();
    }
  }
}