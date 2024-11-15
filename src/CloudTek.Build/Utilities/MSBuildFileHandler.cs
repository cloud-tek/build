using Nuke.Common.IO;

namespace CloudTek.Build.Utilities
{
  /// <summary>
  /// Utility used to parse Directory.Build.(props | targets)
  /// </summary>
  internal class MsBuildFileHandler : XmlFileHandler
  {
#pragma warning disable CA1822
    public IEnumerable<string> GetPinnedPackageVersion(AbsolutePath path)
#pragma warning restore CA1822
    {
      var elements = GetElements(path, "/Project/ItemGroup/PackageVersion")
        .Where(element => element.Attribute("Include") != null)
        .Where(element => element.Attribute("Version")?.Value.ToString().StartsWith('[') ?? false);

      return elements.Select(element => element.Attribute("Include")!.Value);
    }

#pragma warning disable CA1822
    public IEnumerable<string> GetPinnedPackageReference(AbsolutePath path)
#pragma warning restore CA1822
    {
      var elements = GetElements(path, "/Project/ItemGroup/PackageReference")
        .Where(element => element.Attribute("Include") != null)
        .Where(element => element.Attribute("Version")?.Value.ToString().StartsWith('[') ?? false);

      return elements.Select(element => element.Attribute("Include")!.Value);
    }
  }
}