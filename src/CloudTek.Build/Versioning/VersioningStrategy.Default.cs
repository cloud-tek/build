using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning;

public abstract partial class VersioningStrategy
{
  /// <summary>
  /// Default versioning strategy relies on version information found in .csproj files.
  /// When <see cref="SmartBuild.BuildNumber"/> is provided, it is always embedded in the version:
  ///   main/master or no git → {prefix}.{num} (4-part)
  ///   develop                → {prefix}-beta.{num}
  ///   release/*              → {prefix}-rc.{num}
  ///   any other branch       → {prefix}-alpha.{num}
  /// </summary>
  public class Default : VersioningStrategy
  {
    private const string BetaSuffix = "beta";
    private const string RcSuffix = "rc";
    private const string AlphaSuffix = "alpha";

    internal override Func<DotNetPackSettings, SmartBuild, DotNetPackSettings> SetDotNetPackVersion { get; } =
      (settings, build) =>
      {
        var (version, suffix) = Resolve(build);
        if (version != null)
          return settings.SetVersion(version);
        if (suffix != null)
          return settings.SetVersionSuffix(suffix);
        return settings;
      };

    internal override Func<DotNetPublishSettings, SmartBuild, DotNetPublishSettings> SetDotNetPublishVersion { get; } =
      (settings, build) =>
      {
        var (version, suffix) = Resolve(build);
        if (version != null)
          return settings.SetVersion(version);
        if (suffix != null)
          return settings.SetVersionSuffix(suffix);
        return settings;
      };

    private static (string? Version, string? Suffix) Resolve(SmartBuild build)
    {
      var hasBuildNumber = !string.IsNullOrEmpty(build.BuildNumber);
      var buildNumber = hasBuildNumber ? CleanBuildNumber(build.BuildNumber) : string.Empty;
      var git = build.GitRepository;

      var phase = ResolvePhase(git);

      if (phase == Phase.Release)
      {
        if (!hasBuildNumber)
          return (null, null);

        var prefix = ResolveVersionPrefix(build);
        return ($"{prefix}.{buildNumber}", null);
      }

      var suffix = phase switch
      {
        Phase.Beta => BetaSuffix,
        Phase.Rc => RcSuffix,
        Phase.Alpha => AlphaSuffix,
        _ => throw new InvalidOperationException($"Unhandled phase: {phase}")
      };

      return hasBuildNumber
        ? (null, $"{suffix}.{buildNumber}")
        : (null, suffix);
    }

    private static Phase ResolvePhase(GitRepository? git)
    {
      if (git == null) return Phase.Release;
      if (git.IsOnMainBranch() || git.IsOnMasterBranch()) return Phase.Release;
      if (git.IsOnDevelopBranch()) return Phase.Beta;
      if (git.IsOnReleaseBranch()) return Phase.Rc;
      return Phase.Alpha;
    }

    private static string ResolveVersionPrefix(SmartBuild build)
    {
      var prefix = build.Repository.Projects
        .Select(p => p.ProjectProperties.VersionPrefix)
        .FirstOrDefault(v => !string.IsNullOrEmpty(v));

      if (string.IsNullOrEmpty(prefix))
        throw new InvalidOperationException(
          "Unable to resolve VersionPrefix for the 4-part version. " +
          "Set <VersionPrefix> or <Version> in a project or Directory.Build.props.");

      return prefix;
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

    private enum Phase
    {
      Release,
      Beta,
      Rc,
      Alpha
    }
  }
}