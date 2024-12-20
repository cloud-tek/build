using System.Diagnostics;
using CloudTek.Build.Versioning;
using Nuke.Common.IO;

namespace CloudTek.Build.Tool;

/// <summary>
/// The dotnet tool which enables SmartBuild use
/// </summary>
public class Tool : SmartBuild<VersioningStrategy.Default>
{
  /// <summary>
  /// Tool entrypoint
  /// </summary>
  /// <returns></returns>
  public static async Task<int> Main()
  {
    await EnsureNukeFolderExists()
      .ConfigureAwait(false);

    if (LocalBuildExists(out var project))
    {
      var rc = await RunDotnetProcess($"build {project}")
        .ConfigureAwait(false);

      return rc != 0
        ? rc
        : await RunDotnetProcess($"run --project {project}")
        .ConfigureAwait(false);
    }

    return Execute<Tool>(x => x.All);
  }

  private static async Task EnsureNukeFolderExists()
  {
    const string path = ".nuke";
    if (!Directory.Exists(path))
    {
      Directory.CreateDirectory(path);

      await File.WriteAllTextAsync($"{path}/.gitignore", "[Tt]emp/")
        .ConfigureAwait(false);
    }
  }

  private static async Task<int> RunDotnetProcess(string arguments)
  {
    var process = Process.Start(new ProcessStartInfo(
      fileName: "dotnet",
      arguments: arguments));

    await process!
      .WaitForExitAsync()
      .ConfigureAwait(false);

    return process.ExitCode;
  }
  private static bool LocalBuildExists(out string? project)
  {
    const string name = "build";
    const string csExt = ".csproj";
    const string fsExt = ".fsproj";

    project = null;
    var currentDirectory = AbsolutePath.Create(Directory.GetCurrentDirectory());
    if ((currentDirectory / name / $"_{name}.{csExt}").FileExists())
    {
      project = $"{name}/_{name}.{csExt}";
    }

    if ((currentDirectory / name / $"_{name}.{fsExt}").FileExists())
    {
      project = $"{name}/_{name}.{fsExt}";
    }

    return false;
  }
}