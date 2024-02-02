using CloudTek.Build.Utilities;
using Nuke.Common.IO;

namespace CloudTek.Build.Packaging;

/// <summary>
///   Utility used to parse Directory.Build.(props | targets)
/// </summary>
internal sealed class MSBuildFileHandler : XmlFileHandler
{
#pragma warning disable CA1822
  public IEnumerable<(string Project, string Sdk)> GetSdkImports(AbsolutePath path)
  {
    var elements = GetElements(path, "/Project/Import")
      .Where(element => element.Attribute("Project") != null)
      .Where(element => element.Attribute("Sdk") != null);

    return elements.Select(element => (element.Attribute("Project")!.Value!, element.Attribute("Sdk")!.Value));
  }

  public IEnumerable<(string Package, string Version)> GetPackageReferences(AbsolutePath path)
  {
    var elements = GetElements(path, "/Project/ItemGroup/PackageReference");

    return elements.Select(element => (element.Attribute("Include")!.Value!, element.Attribute("Version")!.Value));
  }
#pragma warning restore CA1822
}