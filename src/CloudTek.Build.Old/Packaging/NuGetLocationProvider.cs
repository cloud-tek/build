using Nuke.Common.IO;

namespace CloudTek.Build.Packaging;

internal static class NuGetLocationProvider
{
  internal static AbsolutePath GetPackageLocation()
  {
    // https://learn.microsoft.com/en-us/nuget/reference/nuget-config-file#config-section
    if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
      return $"{Environment.GetEnvironmentVariable("HOME")}/.nuget/packages";

    if (OperatingSystem.IsWindows())
      return "%userprofile%/.nuget/packages";

    throw new NotSupportedException("Unsupported NuGet platform");
  }
}