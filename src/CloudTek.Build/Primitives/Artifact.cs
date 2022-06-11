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
        Package,
        Service
    }

    public class Artifact
    {
        public string Project { get; set; } = default!;
        public string? Module { get; set;}

        private IDictionary<TestType, AbsolutePath> _testProjectPaths = new Dictionary<TestType, AbsolutePath>();

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
            SetPaths(repository.SourceDirectory, repository.TestsDirectory);
            SetAssemblyName();
            SetVersionPrefix();
        }

        private void SetPaths(AbsolutePath sourceDirectory, AbsolutePath testsDirectory)
        {
            if (_path != null) return;

            _path = $"{sourceDirectory}/{Project}/{Project}.csproj";

            _testProjectPaths.Add(TestType.UnitTests, testsDirectory / $"{Project}.Tests.Unit/{Project}.csproj");
            _testProjectPaths.Add(TestType.IntegrationTests,
                 testsDirectory / $"{Project}.Tests.Integration/{Project}.csproj");
        }

        private void SetAssemblyName()
        {
            if (_name != null) return;

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
            if (_versionPrefix != null) return;

            _versionPrefix = AssemblyVersionHandler.Handle(Path);
        }
    }
}