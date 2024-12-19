using System.Text;
using System.Xml;
using System.Xml.Linq;
using Nuke.Common.IO;

namespace CloudTek.Build.Utilities;

  /// <summary>
  /// Utility used to parse Directory.Build.(props | targets)
  /// </summary>
  internal class MsBuildFileHandler : XmlFileHandler
  {
    private const string PropertyGroup = "PropertyGroup";
    // internal MsBuildFileHandler(AbsolutePath path)
    // {
    //   _ = path ?? throw new ArgumentNullException(nameof(path));
    //   _document = GetDocument(path);
    // }
#pragma warning disable CA1822
    public IEnumerable<string> GetPinnedPackageVersion(AbsolutePath path)
#pragma warning restore CA1822
    {
      var doc = GetDocument(path);
      var elements = GetElements(doc, "/Project/ItemGroup/PackageVersion")
        .Where(element => element.Attribute("Include") != null)
        .Where(element => element.Attribute("Version")?.Value.ToString().StartsWith('[') ?? false);

      return elements.Select(element => element.Attribute("Include")!.Value);
    }

#pragma warning disable CA1822
    public IEnumerable<string> GetPinnedPackageReference(AbsolutePath path)
#pragma warning restore CA1822
    {
      var doc = GetDocument(path);
      var elements = GetElements(doc, "/Project/ItemGroup/PackageReference")
        .Where(element => element.Attribute("Include") != null)
        .Where(element => element.Attribute("Version")?.Value.ToString().StartsWith('[') ?? false);

      return elements.Select(element => element.Attribute("Include")!.Value);
    }

    public static bool GetIsPackable(AbsolutePath path)
    {
      var doc = GetDocument(path);
      var element = GetElements(doc, "/Project/PropertyGroup/IsPackable")
        .SingleOrDefault();

      return element != null && bool.Parse(element.Value);
    }

    public static void SetIsPackable(AbsolutePath path, bool value, bool createIfNotExists = true, AbsolutePath? output = null)
    {
      var doc = GetDocument(path: path, createIfNotExists: createIfNotExists);
      var root = doc.Root;

      if (root == null)
      {
        throw new InvalidOperationException("Root element <Project /> is missing");
      }

      var isPackable = GetIsPackable(path);

      if (isPackable != value)
      {
        var pg = root!.Element(PropertyGroup);
        if (pg == null)
        {
          pg = new XElement(PropertyGroup);
          root.Add(pg);
        }

        pg.SetElementValue("IsPackable", value);
      }

      var settings = new XmlWriterSettings
      {
        OmitXmlDeclaration = true,
        Indent = true,
        Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
      };

      using var xw = XmlWriter.Create(output ?? path, settings);
      doc.Save(xw);
    }
}