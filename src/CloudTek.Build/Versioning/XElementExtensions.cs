using System.Xml.Linq;

namespace CloudTek.Build.Versioning
{
    public static class XElementExtensions
    {
        public static bool HasAttribute(this XElement element, string name)
        {
            return element.Attribute(name) != null;
        }
    }
}