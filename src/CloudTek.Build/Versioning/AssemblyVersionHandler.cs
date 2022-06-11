namespace CloudTek.Build.Versioning
{
    public static class AssemblyVersionHandler
    {
        private static readonly IAssemblyVersionHandler[] Handlers = {
            new CsProjVersionHandler(),
            new CsProjVersionPrefixHandler(),
            new TargetsVersionHandler(),
            new TargetsVersionSuffixHandler()
        };
        
        public static string Handle(string path)
        {
            const string defaultVersion = "0.0.0";
            var version = defaultVersion;
            
            foreach (var handler in Handlers)
            {
                version = handler.Handle(path) ?? version;
                
                if(version != defaultVersion) break;
            }

            return version;
        }
    }
}