using CloudTek.Build;
using CloudTek.Build.Versioning;

// ReSharper disable once CheckNamespace
namespace _build;

public class Build : SmartBuild<VersioningStrategy.Default>
{
  // todo: restore once versioniong is fixed
  // /// <summary>
  // /// GitVersion information for SmartBuild (disabled)
  // /// </summary>
  // [GitVersion(Framework = "net8.0", NoFetch = true)]
  // // ReSharper disable once UnusedAutoPropertyAccessor.Global
  // public GitVersion GitVersion { get; set; } = default!;

  public override string PackagesFilter { get; init; } = "Nuke";

  public static int Main() => Execute<Build>(x => x.Compile);
}