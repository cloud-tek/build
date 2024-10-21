using NuGet.Packaging;
using Nuke.Common;
using Serilog;

namespace CloudTek.Build;

internal static class TargetDefinitionExtensions
{
  public const string NukeSkipList = "NUKE_SKIP";
  public const string NukeSkipPrefix = "NUKE_SKIP_";

  private static ImmutableHashSet<string> GetSkippedTargets(IReadOnlyDictionary<string, string> environment)
  {
    var result = new HashSet<string>();

    if (environment.TryGetValue(NukeSkipList, out var nukeSkip))
    {
      if (!string.IsNullOrWhiteSpace(nukeSkip))
      {
        result.AddRange(
          nukeSkip.Split(
            separator: ' ',
            options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
      }
    }

    result.AddRange(
      environment
        .Where(kvp => kvp.Key.StartsWith(NukeSkipPrefix, StringComparison.OrdinalIgnoreCase))
        .Where(kvp => bool.TryParse(kvp.Value, out var value) && value)
        .Select(kvp => kvp.Key.Replace(NukeSkipPrefix, string.Empty)));

    return result.ToImmutableHashSet();
  }

  internal static void PrintTargetsToSkip(IReadOnlyDictionary<string, string> environment)
  {
    var skippedTargets = GetSkippedTargets(environment);

    Log.Information("Following targets will be skipped due to env variable NUKE_SKIP or NUKE_SKIP_<TARGET>: ");
    foreach (var target in skippedTargets!)
    {
      Log.Information(target);
    }
  }

  internal static bool ShouldSkipTarget(
    string targetName,
    IReadOnlyCollection<string> invokedTargets,
    IReadOnlyDictionary<string, string> environment)
  {
    var skippedTargets = GetSkippedTargets(environment);

    if (invokedTargets.Any(p => p == targetName))
      return false;

    var shouldSkip = skippedTargets!.Contains(targetName, StringComparer.OrdinalIgnoreCase);

    return shouldSkip;
  }

  internal static ITargetDefinition CheckIfSkipped(
    this ITargetDefinition definition,
    string targetName,
    SmartBuild build) =>
    definition.OnlyWhenStatic(
      () => !ShouldSkipTarget(
        targetName,
        build.InvokedTargets.Select(p => p.Name).ToList(),
        build.EnvironmentVariables));
}