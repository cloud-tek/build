using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace CloudTek.Build.Versioning
{
    public class CsProjVersionHandler : XmlVersionHandler, IAssemblyVersionHandler
    {
        public string Handle(string path)
        {
            return base.Handle(path, "/Project/PropertyGroup/Version");
        }
    }
}