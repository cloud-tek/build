using System.Text.RegularExpressions;

namespace CloudTek.Build.Packaging;

internal static class StringExtensions
{
#pragma warning disable MA0009
  // Matches 'dotnet list package' std output: "Project 'SomeService' has the following package references"
  private static readonly Regex ProjectOutputRegex =
    new(@"^Project\s\'(\S*)\'\shas\sthe\sfollowing\spackage\sreferences$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

  private static readonly Regex ProjectOutputOutdatedRegex =
    new(@"^Project\s\`(\S*)\`\shas\sthe\sfollowing\supdates\sto\sits\spackages$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

  // Matches 'dotnet list package' std output: " > AsyncFixer                                      1.6.0            1.6.0      "
  private static readonly Regex PackageOutputRegex =
    new(@"^\s*>\s(\S*)\s*(\S*)\s*(\S*)\s*(\S*)\s*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

  // Matches 'dotnet list pacakge' std output: "The given project `SomeProject` has no updates given the current sources."
  private static readonly Regex NoUpdatesOutputRegex =
    new(@"^The\sgiven\sproject\s\`(\S*)\`\shas\sno\supdates\sgiven\sthe\scurrent\ssources\.$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
#pragma warning restore MA0009
  internal static bool IsProjectOutput(this string output, PackageParsingMode mode, out string project)
  {
    if (string.IsNullOrEmpty(output))
    {
      project = string.Empty;
      return false;
    }

    var regex = mode == PackageParsingMode.Outdated
      ? ProjectOutputOutdatedRegex
      : ProjectOutputRegex;

    var result = regex.IsMatch(output);

    project = result ? regex.Match(output).Groups[1].Value : string.Empty;

    return result;
  }

  internal static bool IsPackageOutput(this string output, Func<string, bool> packagePinResolver, out Package package)
  {
    var match = PackageOutputRegex.Match(output);

    // If LATEST isn't provided, assume RESOLVED is LATEST
    // Why:
    // - dotnet list package doesn't return LATEST
    // - dotnet list package --outdated doesn't return the output if it's already LATEST
    var latest = string.IsNullOrEmpty(match.Groups[4].Value)
      ? match.Groups[3].Value
      : match.Groups[4].Value;

    package = match.Success
      ? new Package
      {
        Name = match.Groups[1].Value,
        Requested = match.Groups[2].Value,
        Resolved = match.Groups[3].Value,
        Latest = latest,
        IsPinned = packagePinResolver(match.Groups[1].Value)
      }
      : default!;

    return match.Success;
  }

  internal static bool IsNoUpdatesOutput(this string output, out string project)
  {
    if (string.IsNullOrEmpty(output))
    {
      project = string.Empty;
      return false;
    }

    var match = NoUpdatesOutputRegex.Match(output);

    project = match.Success ? NoUpdatesOutputRegex.Match(output).Groups[1].Value : string.Empty;

    return match.Success;
  }
}