using Nuke.Common.Tools.DotNet;
using System;
using CloudTek.Build.Primitives;

namespace CloudTek.Build.Versioning
{
    public abstract partial class VersioningStrategy
    {
        public abstract Func<DotNetBuildSettings, SmartBuild, Artifact, DotNetBuildSettings> SetDotNetBuildVersion { get; }
        public abstract Func<DotNetPublishSettings, SmartBuild, Artifact, DotNetPublishSettings> SetDotNetPublishVersion { get; }
        public abstract Func<DotNetPackSettings, SmartBuild, Artifact, DotNetPackSettings> SetDotNetPackVersion { get; }
        public abstract Func<DotNetNuGetPushSettings, SmartBuild, Artifact, DotNetNuGetPushSettings> SetDotNetNuGetPushVersion { get; }
    }
}