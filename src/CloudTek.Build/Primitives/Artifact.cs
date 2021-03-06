using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using CloudTek.Build.Versioning;
using Nuke.Common.IO;

namespace CloudTek.Build.Primitives
{
    public enum ArtifactType
    {
        /// <summary>
        /// Artifact located in /src or /{module}/src that is going to be emitted as a NuGet package
        /// </summary>
        Package = 0,

        /// <summary>
        /// Artifact located in /src or /{module}/src that is going to be emitted as a container image
        /// </summary>
        Service,

        /// <summary>
        /// Artifact located in /demo or /{module}/demo that is going to be emitted as a container image
        /// </summary>
        Demo
    }

    public class Artifact : RepositoryContent
    {
        public string Project { get; set; } = default!;

        private string _path = default!;

        public string Path
        {
            get
            {
                if (_path == null)
                {
                    throw new InvalidOperationException(
                        "Artifact name has not been computed yet. Call .SetSourceDirectory() first");
                }

                return _path;
            }
        }

        private string _versionPrefix = default!;

        public string VersionPrefix
        {
            get
            {
                if (_versionPrefix == null)
                {
                    throw new InvalidOperationException(
                        "Artifact name has not been computed yet. Call .SetSourceDirectory() first");
                }

                return _versionPrefix;
            }
        }

        public ArtifactType Type { get; set; }

        private string _name = default!;

        public string Name
        {
            get
            {
                if (_name == null)
                {
                    throw new InvalidOperationException(
                        "Artifact name has not been computed yet. Call .SetSourceDirectory() first");
                }

                return _name;
            }
        }

        public void Initialize(Repository repository)
        {
            SetPaths(repository);
            SetAssemblyName();
            SetVersionPrefix();
        }

        private void SetPaths(Repository repository)
        {
            if (!string.IsNullOrEmpty(_path)) return;

            if (Type == ArtifactType.Demo)
            {
              _path = string.IsNullOrEmpty(Module)
                ? $"{repository.DemoDirectory}/{Project}/{Project}.csproj"
                : $"{repository.RootDirectory}/{Module}/demo/{Project}/{Project}.csproj";
              return;
            }

            _path = string.IsNullOrEmpty(Module)
                ? $"{repository.SourceDirectory}/{Project}/{Project}.csproj"
                : $"{repository.RootDirectory}/{Module}/src/{Project}/{Project}.csproj";
        }

        private void SetAssemblyName()
        {
            if (!string.IsNullOrEmpty(_name)) return;

            using var reader = new StreamReader(_path);
            var doc = XDocument.Load(reader);
            var element = doc.XPathSelectElement("/Project/PropertyGroup/AssemblyName");

            if (element != null)
            {
                _name = element.Value;
            }
            else
            {
                _name = Project;
            }

            Serilog.Log.Debug($"{Project} has output assembly: {_name}");
        }

        private void SetVersionPrefix()
        {
            if (!string.IsNullOrEmpty(_versionPrefix)) return;

            _versionPrefix = AssemblyVersionHandler.Handle(Path);
        }
    }
}
