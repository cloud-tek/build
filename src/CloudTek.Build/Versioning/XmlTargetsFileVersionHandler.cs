using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CloudTek.Build.Versioning
{
    public class XmlTargetsFileVersionHandler : XmlVersionHandler
    {
        protected const string ProjectAttribute = "Project";
        private const string TargetsFileEnding = "Version.targets";


        public string? Handle(string path, string element)
        {
            var imports = base.GetElements(path, "/Project/Import");
            var import = imports?.SingleOrDefault(import => 
                (import.Attribute(ProjectAttribute) is { } attribute) 
                && attribute.Value.EndsWith(TargetsFileEnding));

            if (import == null) return null;
            
            var targetPath = import.Attribute(ProjectAttribute);

            var p = Path.GetDirectoryName(path);
            string absoluteTargetPath;
                
            // Version.targets explicitly relative to the .csproj file
            if (targetPath != null && (targetPath.Value.StartsWith("../") || targetPath.Value.StartsWith("..\\")))
            {
                absoluteTargetPath = Path.Join(p, targetPath.Value.Replace("..\\", "../"));
            } 
            else if (targetPath != null && targetPath.Value.StartsWith("./"))
            {
                absoluteTargetPath = Path.Join(p, targetPath.Value.Replace("./", "/"));
            }
            // Version.targets implicitly relative to the .csproj file "Project"="Version.targets"
            else
            {
                absoluteTargetPath = Path.Join(p, targetPath?.Value);
            }
                
            return base.Handle(absoluteTargetPath, $"/Project/PropertyGroup/{element}");

        }
    }
}