using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CloudTek.Build.Versioning
{
    public abstract class XmlVersionHandler
    {
        protected const string VersionElement = "Version";
        protected const string VersionPrefixElement = "VersionPrefix";
        protected string? Handle(string path, params string[] args)
        {
            var element = GetElement(path, args[0]);

            if (element != null)
            {
                return element.Value;
            }

            return null;
        }

        protected XElement? GetElement(string path, string xpath)
        {
            using var reader = new StreamReader(path);
            var doc = XDocument.Load(reader);
            return doc.XPathSelectElement(xpath);
        }
        
        protected IEnumerable<XElement> GetElements(string path, string xpath)
        {
            using var reader = new StreamReader(path);
            var doc = XDocument.Load(reader);
            return doc.XPathSelectElements(xpath);
        }
    }
}