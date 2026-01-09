using CloudTek.Build.Extensions;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning;

public abstract partial class VersioningStrategy
{
  /// <summary>
  /// Default versioning strategy relies on version information found in .csproj files
  /// </summary>
  public class Default : VersioningStrategy
  {
    private const string BetaSuffix = "beta";

    internal override Func<DotNetPackSettings, SmartBuild, DotNetPackSettings> SetDotNetPackVersion { get; } =
      (settings, build) =>
        settings
          .ExecuteWhen(
            build.Repository.ShouldAddBetaSuffix(build.GitRepository),
            (s) => s
              .SetVersionSuffix(
                GetBetaBuildSuffix(build.GitRepository!, BetaSuffix, build.BuildNumber)));

    internal override Func<DotNetPublishSettings, SmartBuild, DotNetPublishSettings> SetDotNetPublishVersion { get; } =
      (settings, build) =>
        settings
          .ExecuteWhen(
            build.Repository.ShouldAddBetaSuffix(build.GitRepository),
            (s) => s
              .SetVersionSuffix(
                GetBetaBuildSuffix(build.GitRepository!, BetaSuffix, build.BuildNumber)));

    private static string GetBetaBuildSuffix(GitRepository git, string suffix, string buildNumber)
    {
      if (string.IsNullOrEmpty(suffix))
        throw new ArgumentNullException(nameof(suffix));

      if (string.IsNullOrEmpty(buildNumber) || git.IsOnDevelopBranch())
        return suffix;

      return $"{suffix}.{CleanBuildNumber(buildNumber)}";
    }

    internal static string CleanBuildNumber(string buildNumber)
    {
      if (buildNumber.Contains("merge_"))
      {
        return buildNumber.Replace("merge_", string.Empty);
      }
      else
      {
        return buildNumber.Split("_").Last();
      }
    }
  }
}