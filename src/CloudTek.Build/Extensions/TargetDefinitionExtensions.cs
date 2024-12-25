using Nuke.Common;
using Serilog;

namespace CloudTek.Build.Extensions;

internal static class TargetDefinitionExtensions
{
  internal static void PrintTargetsToSkip()
  {
    var nukeSkip = EnvironmentInfo.Variables.GetValueOrDefault("NUKE_SKIP");

    if (nukeSkip != null && !string.IsNullOrWhiteSpace(nukeSkip))
    {
      Log.Information("Following targets will be skipped due to env variable NUKE_SKIP");
      Log.Information(nukeSkip);
    }
  }
}