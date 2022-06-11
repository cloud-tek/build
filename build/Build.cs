using CloudTek.Build;
using CloudTek.Build.Primitives;
using CloudTek.Build.Versioning;
using Nuke.Common.Execution;
using Nuke.Common.Tools.GitVersion;

// ReSharper disable once CheckNamespace
namespace _build
{
    [CheckBuildProjectConfigurations]
    public class Build : SmartGitVersionBuild
    {
        public static int Main () => Execute<Build>(x => x.Compile);
        
        public Build() : base(Repository)
        { }

        static new readonly Repository Repository = new ()
        {
            Artifacts = new []
            {
                new Artifact() { Type = ArtifactType.Package, Project = "CloudTek.Build" }
            }
        };
    }
}