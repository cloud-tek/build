using System.Reflection;
using System.Text;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build;

public abstract partial class SmartBuild
{
  /// <summary>
  /// dotnet nuke --target UpdateOutdated
  /// Runs dotnet outdated to update packages with --packages-filter defaulted to packages matching the package filter
  /// </summary>
  protected virtual Target UpdateOutdated => _ => _
    .Description("Run dotnet outdated to update packages with --packages-filter defaulted to packages matching the package filter")
    .DependsOn(DotnetOutdatedInstall)
    .Executes(
      () =>
      {
        var command = "outdated -u";
        foreach (var include in PackagesFilter.Split(
                   " ",
                   StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
          command += " --include " + include;
        }

        DotNet(command, Solution.Directory);
        PackageManager.CheckOutdatedPackages();
      });

  // TODO: evaluate if this is needed. refactor
  // protected virtual Target SetupCustom => _ => _
  //   .Description("This target sets up a custom build definition project to extend or override build logic")
  //   .Executes(
  //     async () =>
  //     {
  //       var buildPath = RootDirectory / "build";
  //
  //       if (await CopyResourcesIfFolderDoesntExist(buildPath, ".template.build"))
  //       {
  //         DotNet($"add build/_build.csproj package CloudTek.Build", Repository.RootDirectory);
  //
  //         Log.Information("Custom definition setup done! Take a look at the build directory.");
  //       }
  //       else
  //       {
  //         Log.Information("There is already a build directory, skipping setup");
  //       }
  //     });

  // TODO: refactor/move
  private static async Task<bool> CopyResourcesIfFolderDoesntExist(
    AbsolutePath folderPath,
    string resourcePrefix,
    Dictionary<string, string>? replaceTags = null)
  {
    if (Directory.Exists(folderPath))
    {
      return false;
    }

    await CopyResources(folderPath, resourcePrefix, replaceTags);
    return true;
  }

  // TODO: refactor/move
  private static async Task CopyResources(
    AbsolutePath folderPath,
    string resourcePrefix,
    Dictionary<string, string>? replaceTags = null)
  {
    if (!Directory.Exists(folderPath))
    {
      Directory.CreateDirectory(folderPath);
    }

    var asm = Assembly.GetExecutingAssembly();
    var resources = asm.GetManifestResourceNames().Where(p => p.Contains(resourcePrefix));
    foreach (var strResourceName in resources)
    {
      await using var resourceStream = asm.GetManifestResourceStream(strResourceName);
      if (resourceStream == null)
      {
        continue;
      }

      using var sRdr = new StreamReader(resourceStream);
      var strTxt = await sRdr.ReadToEndAsync();
      var targetFileName =
        FixPath(strResourceName.Replace(asm.GetName().Name + $".{resourcePrefix}.", ""));
      targetFileName = ReplaceTags(targetFileName, replaceTags);
      var targetFile = folderPath / targetFileName;
      var directory = Path.GetDirectoryName(targetFile);

      if (directory != null)
      {
        Directory.CreateDirectory(directory);
      }

      await File.WriteAllTextAsync(targetFile, ReplaceTags(strTxt, replaceTags));
    }
  }

  // TODO: refactor/move
  private static string FixPath(string weirdString)
  {
    var lastDotIndex = weirdString.LastIndexOf('.');

    if (lastDotIndex != -1)
    {
      // replace dots with slashes except when this would create two slashes in a row or the dot is escaped with a dollar sign
      weirdString = weirdString
        .ReplaceRegex(@"([^\\$])\.", x => $"{x.Groups[1]}/");

      var sb = new StringBuilder(weirdString);
      sb[lastDotIndex] = '.';
      weirdString = sb.ToString()
        .Replace("$.", ".");
    }

    return weirdString.Replace("__", "_");
  }

  // TODO: refactor/move
  private static string ReplaceTags(string input, Dictionary<string, string>? replaceTags)
  {
    if (replaceTags == null)
    {
      return input;
    }

    var sb = new StringBuilder(input);

    foreach (var (key, value) in replaceTags)
    {
      sb.Replace(key, value);
    }

    return sb.ToString();
  }
}