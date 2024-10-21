using CloudTek.Build;
using CloudTek.Build.Versioning;
using Nuke.Common.Tools.GitVersion;

// ReSharper disable once CheckNamespace
namespace _build;

public class Build : SmartBuild<VersioningStrategy.GitVersion>
{
  /// <summary>
  /// GitVersion information for SmartBuild
  /// </summary>
  [GitVersion(Framework = "net8.0", NoFetch = true)]
  // ReSharper disable once UnusedAutoPropertyAccessor.Global
  public GitVersion GitVersion { get; set; } = default!;

  // TODO: ensure this is override'able
  // public override Regex PackageChecksRegex { get; init; } = new Regex("^Nuke", RegexOptions.Compiled);

  public static int Main() => Execute<Build>(x => x.Compile);
}