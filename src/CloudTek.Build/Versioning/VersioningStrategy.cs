using CloudTek.Build.Primitives;
using Nuke.Common.Tools.DotNet;

namespace CloudTek.Build.Versioning;

public abstract partial class VersioningStrategy
{
  internal abstract Func<DotNetPublishSettings, SmartBuild, Project, DotNetPublishSettings> SetDotNetPublishVersion
  {
    get;
  }

  internal abstract Func<DotNetPackSettings, SmartBuild, Project, DotNetPackSettings> SetDotNetPackVersion { get; }

  internal abstract Func<DotNetNuGetPushSettings, SmartBuild, Project, DotNetNuGetPushSettings> SetDotNetNuPkgPath
  {
    get;
  }
}