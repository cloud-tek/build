using System;
using CloudTek.Build.Primitives;
using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning
{
    public abstract partial class VersioningStrategy
    {
        public class GitVersion : VersioningStrategy
        {
            public override Func<DotNetBuildSettings, SmartBuild, Artifact, DotNetBuildSettings> SetDotNetBuildVersion
            {
                get;
            } = (settings, build, artifact) =>
            {
                var bld = (SmartGitVersionBuild)build;

                return settings
                    .SetVersion(bld.GitVersion.NuGetVersionV2)
                    .SetFileVersion(bld.GitVersion.AssemblySemFileVer)
                    .SetAssemblyVersion(bld.GitVersion.AssemblySemVer);
            };

            public override Func<DotNetPublishSettings, SmartBuild, Artifact, DotNetPublishSettings>
                SetDotNetPublishVersion { get; } = (settings, build, artifact) =>
            {
                var bld = (SmartGitVersionBuild)build;

                return settings
                    .SetVersion(bld.GitVersion.NuGetVersionV2)
                    .SetFileVersion(bld.GitVersion.AssemblySemFileVer)
                    .SetAssemblyVersion(bld.GitVersion.AssemblySemVer);
            };

            public override Func<DotNetPackSettings, SmartBuild, Artifact, DotNetPackSettings> SetDotNetPackVersion
            {
                get;
            } = (settings, build, artifact) =>
            {
                var bld = (SmartGitVersionBuild)build;

                return settings
                    .SetVersion(bld.GitVersion.NuGetVersionV2)
                    .SetFileVersion(bld.GitVersion.AssemblySemFileVer)
                    .SetAssemblyVersion(bld.GitVersion.AssemblySemVer);
            };

            public override Func<DotNetNuGetPushSettings, SmartBuild, Artifact, DotNetNuGetPushSettings>
                SetDotNetNuGetPushVersion { get; } = (settings, build, artifact) =>
            {
                var bld = (SmartGitVersionBuild)build;

                return settings
                    .SetTargetPath(bld.Repository.PackagesDirectory / artifact.Name /
                                   $"{artifact.Name}.{bld.GitVersion.NuGetVersionV2}.nupkg");
            };
        };
    }
}