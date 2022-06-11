namespace CloudTek.Build.Versioning
{
    public class CsProjVersionPrefixHandler : XmlVersionHandler, IAssemblyVersionHandler
    {
        public string Handle(string path)
        {
            return base.Handle(path, "/Project/PropertyGroup/VersionPrefix");
        }
    }
}