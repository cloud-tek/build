using System;
using CloudTek.Build.Primitives;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning
{
    public abstract partial class VersioningStrategy
    {
        public class Default : VersioningStrategy
        {

            private const string BetaSuffix = "beta";

            public Default()
            {
            }

            public override Func<DotNetBuildSettings, SmartBuild, Artifact, DotNetBuildSettings>
                SetDotNetBuildVersion
            { get; } =
                (settings, build, artifact) =>

                    settings
                        .When(artifact.Type == ArtifactType.Package && build.Repository.ShouldEmitPackage(build.GitRepository),
                            (settings) => settings
                                .SetVersionPrefix(artifact.VersionPrefix))
                        .When(artifact.Type == ArtifactType.Package && build.Repository.ShouldEmitBetaPackage(build.GitRepository),
                            (settings) => settings
                                .SetVersionPrefix(artifact.VersionPrefix)
                                .SetVersionSuffix(GetBetaBuildSuffix(build.GitRepository, BetaSuffix, build.BuildNumber)));

            public override Func<DotNetPublishSettings, SmartBuild, Artifact, DotNetPublishSettings> SetDotNetPublishVersion { get; } =
                (settings, build, artifact) =>
                    settings
                        .When(artifact.Type == ArtifactType.Package && build.Repository.ShouldEmitPackage(build.GitRepository),
                            (settings) => settings
                                .SetVersionPrefix(artifact.VersionPrefix))
                        .When(artifact.Type == ArtifactType.Package && build.Repository.ShouldEmitBetaPackage(build.GitRepository),
                            (settings) => settings
                                .SetVersionPrefix(artifact.VersionPrefix)
                                .SetVersionSuffix(GetBetaBuildSuffix(build.GitRepository, BetaSuffix, build.BuildNumber)));

            public override Func<DotNetPackSettings, SmartBuild, Artifact, DotNetPackSettings> SetDotNetPackVersion { get; } =
                (settings, build, artifact) =>
                    settings
                        .When(artifact.Type == ArtifactType.Package && build.Repository.ShouldEmitPackage(build.GitRepository),
                            (settings) => settings
                                .SetVersionPrefix(artifact.VersionPrefix))
                        .When(artifact.Type == ArtifactType.Package && build.Repository.ShouldEmitBetaPackage(build.GitRepository),
                            (settings) => settings
                                .SetVersionPrefix(artifact.VersionPrefix)
                                .SetVersionSuffix(GetBetaBuildSuffix(build.GitRepository, BetaSuffix, build.BuildNumber)));

            public override Func<DotNetNuGetPushSettings, SmartBuild, Artifact, DotNetNuGetPushSettings> SetDotNetNuGetPushVersion { get; } =
                (settings, build, artifact) =>
                    settings
                        .When(artifact.Type == ArtifactType.Package && build.Repository.ShouldEmitPackage(build.GitRepository), (sttngs) => sttngs
                               .SetTargetPath(build.Repository.PackagesDirectory / artifact.Name /
                                               $"{artifact.Name}.{artifact.VersionPrefix}.nupkg"))
                        .When(artifact.Type == ArtifactType.Package && build.Repository.ShouldEmitBetaPackage(build.GitRepository), (sttngs) => sttngs
                                .SetTargetPath(build.Repository.PackagesDirectory / artifact.Name /
                                               $"{artifact.Name}.{artifact.VersionPrefix}-{GetBetaBuildSuffix(build.GitRepository, BetaSuffix, build.BuildNumber)}.nupkg"));

            private static string GetBetaBuildSuffix(GitRepository git, string suffix, string buildNumber)
            {
                if (suffix == null) throw new ArgumentException(nameof(suffix));

                return (string.IsNullOrEmpty(buildNumber)
                    ? suffix
                    : git.IsOnDevelopBranch()
                        ? suffix
                        : $"{suffix}{buildNumber}");
            }
        }
    }
}