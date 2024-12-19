using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Nuke.Common.IO;

namespace CloudTek.Build.Utilities
{
  internal abstract class XmlFileHandler
  {
    public static XDocument GetDocument(AbsolutePath path, bool createIfNotExists = true)
    {
      if (!path.FileExists() && createIfNotExists)
      {
        if (createIfNotExists)
        {
          var doc = new XDocument();
          doc.Add(new XElement("Project"));

          return doc;
        }

        throw new FileNotFoundException($"The path '{path}' does not exist");
      }

      using var reader = new StreamReader(path);
      return XDocument.Load(reader);
    }
    protected static IEnumerable<XElement> GetElements(XDocument document, string xpath)
    {
      return document.XPathSelectElements(xpath);
    }
  }
}