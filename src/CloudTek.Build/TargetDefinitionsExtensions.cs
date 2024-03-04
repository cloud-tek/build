using NuGet.Packaging;
using Nuke.Common;
using Serilog;

namespace CloudTek.Build;

internal static class TargetDefinitionExtensions
{
  private const string NukeSkipList = "NUKE_SKIP";
  private const string NukeSkipPrefix = "NUKE_SKIP_";

  private static HashSet<string>? skippedTargets;

  private static void GetSkippedTargets()
  {
    if (skippedTargets == null)
    {
      skippedTargets = new HashSet<string>();

      var nukeSkip = EnvironmentInfo.Variables.GetValueOrDefault(NukeSkipList);
      if (nukeSkip != null)
      {
        if (!string.IsNullOrWhiteSpace(nukeSkip))
        {
          skippedTargets.AddRange(nukeSkip.Split(
            separator: ' ',
            options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }
      }

      skippedTargets.AddRange(EnvironmentInfo.Variables
          .Where(p => p.Key
            .StartsWith(NukeSkipPrefix, StringComparison.OrdinalIgnoreCase)
                      && bool.TryParse((string?)p.Value, out var skip) && skip)
          .Select(p => p.Key.Remove(0, NukeSkipPrefix.Length)));
    }
  }

  internal static void PrintTargetsToSkip()
  {
    GetSkippedTargets();

    Log.Information("Following targets will be skipped due to env variable NUKE_SKIP or NUKE_SKIP_<TARGET>: ");
    foreach (var target in skippedTargets!)
    {
      Log.Information(target);
    }
  }

  internal static bool ShouldSkipTarget(string targetName, IReadOnlyCollection<string> invokedTargets)
  {
    GetSkippedTargets();

    if (invokedTargets.Any(p => p == targetName))
      return false;

    var shouldSkip = skippedTargets!.Contains(targetName, StringComparer.OrdinalIgnoreCase);

    return shouldSkip;
  }

  internal static ITargetDefinition BaseTarget(this ITargetDefinition definition, string targetName, NukeBuild build) =>
      definition.OnlyWhenStatic(() => !ShouldSkipTarget(targetName, build.InvokedTargets.Select(p => p.Name).ToList()));
}