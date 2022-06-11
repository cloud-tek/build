using CloudTek.Build.Versioning;
using Nuke.Common.Tools.GitVersion;

namespace CloudTek.Build
{
    public abstract class SmartGitVersionBuild : SmartBuild<VersioningStrategy.GitVersion>
    {
        protected SmartGitVersionBuild(Repository repository) : base(repository)
        {
        }

        [GitVersion(Framework = "net5.0", NoFetch = true)]
        public GitVersion GitVersion { get; set; } = default!;
    }    
}