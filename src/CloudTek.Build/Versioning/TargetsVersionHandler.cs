using System.IO;
using System.Linq;

namespace CloudTek.Build.Versioning
{
    public class TargetsVersionHandler : XmlTargetsFileVersionHandler, IAssemblyVersionHandler
    {
        public string Handle(string path)
        {
            return base.Handle(path, VersionElement);
        }
    }
}