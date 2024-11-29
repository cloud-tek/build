using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CloudTek.Build.Utilities
{
  internal abstract class XmlFileHandler
  {
    protected static IEnumerable<XElement> GetElements(string path, string xpath)
    {
      using var reader = new StreamReader(path);
      var doc = XDocument.Load(reader);
      return doc.XPathSelectElements(xpath);
    }
  }
}