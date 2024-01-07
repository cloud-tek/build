using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build.Packaging;

public abstract partial class PackageManager
{
  /// <summary>
  /// The default NuGet package manager
  /// </summary>
  public sealed class NuGet : PackageManager
  {
    private const string PackageReferencesXpath = "/Project/ItemGroup/PackageReference";
    private const string PackageVersionAttribute = "Version";
    private const string PackageNameAttribute = "Include";

    // <PackageReference Include="xunit.runner.visualstudio" Version="[2.4.3]"> <-- pinned nuget version
#pragma warning disable MA0009
    private static readonly Regex PinnedNugetVersionRegex = new(@"^\[\S*\]$", RegexOptions.Compiled);
#pragma warning restore MA0009

    /// <summary>
    /// Restores the solution packages
    /// </summary>
    /// <param name="build"></param>
    public override void Restore(SmartBuild build)
    {
      Log.Debug("Restoring packages in solution ...");
      build.Repository.Artifacts.ForEach(artifact => { artifact.Initialize(); });

      DotNetRestore(x => x
        .SetProcessWorkingDirectory(build.Solution.Directory)
        .SetProcessToolPath(DotNetPath));
    }

    internal override ISet<string> GetPinnedPackages(SmartBuild build, string project)
    {
      var prj = build.Solution.AllProjects.SingleOrDefault(p => p.Name == project);

      if (prj != null)
        return GetElements(prj.Path, PackageReferencesXpath)
          .Where(p =>
          {
            var attribute = p.Attribute(PackageVersionAttribute);
            return attribute != null && PinnedNugetVersionRegex.IsMatch(attribute.Value);
          })
          .Select(p =>
          {
            var attribute = p.Attribute(PackageNameAttribute);
            return attribute?.Value ?? throw new InvalidOperationException(
              $"Package reference does not include the {PackageNameAttribute} attribute or it is empty");
          })
          .ToHashSet(StringComparer.InvariantCulture);

      return new HashSet<string>();
    }

    private static IEnumerable<XElement> GetElements(string path, string xpath)
    {
      using var reader = new StreamReader(path);
      var doc = XDocument.Load(reader);
      return doc.XPathSelectElements(xpath);
    }
  }
}