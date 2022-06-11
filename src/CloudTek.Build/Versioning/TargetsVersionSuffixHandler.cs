using System.IO;
using System.Linq;

namespace CloudTek.Build.Versioning
{
    public class TargetsVersionSuffixHandler : XmlTargetsFileVersionHandler, IAssemblyVersionHandler
    {
        public string? Handle(string path)
        {
            return base.Handle(path, VersionPrefixElement);
        }
    }
}