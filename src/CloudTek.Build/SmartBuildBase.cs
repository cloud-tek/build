using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using System.Linq;
using CloudTek.Build.Primitives;
using Nuke.Common.Git;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using CloudTek.Build.Versioning;

namespace CloudTek.Build
{
    public abstract class SmartBuild<TVersioningStrategy> : SmartBuild
        where TVersioningStrategy : VersioningStrategy, new()
    {
        protected SmartBuild(Repository repository) : base(repository, new TVersioningStrategy())
        {
        }
    }

    public abstract class SmartBuild : NukeBuild
    {        
        public readonly Repository Repository;

        protected readonly VersioningStrategy VersioningStrategy;

        protected SmartBuild(Repository repository, VersioningStrategy versioningStrategy)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (repository.Artifacts == null || repository.Artifacts.Length == 0)
                throw new ArgumentException("The repository needs to specify at least 1 buildable artifact");

            Repository = repository;
            Repository.Initialize(RootDirectory);
            VersioningStrategy = versioningStrategy;
        }

        [Parameter("Configuration to _build - Default is 'Debug' (local) or 'Release' (server)")]
        public Configuration Configuration { get; set; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Parameter("BuildNumber")]
        public string BuildNumber { get; set; } = default!;

        [Parameter] public string NugetApiUrl { get; set; } = default!;

        [Parameter] public string NugetApiKey { get; set; } = default!;

        //[Solution] readonly Solution Solution;
        [GitRepository] public GitRepository GitRepository { get; set; }= default!;

        protected Target Clean => _ => _
            .Before(Restore)
            .Executes(() =>
            {
                EnsureCleanDirectory(Repository.ArtifactsDirectory);
                EnsureCleanDirectory(Repository.TestResultsDirectory);
                EnsureCleanDirectory(Repository.TestCoverageDirectory);
            });

        protected Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() =>
            {
                Repository.Artifacts.ForEach(artifact =>
                {
                    artifact.Initialize(Repository);

                    Serilog.Log.Debug($"Restoring {artifact.Project}");
                    DotNetRestore(s => s
                        .SetProjectFile(artifact.Path)
                        .SetProcessToolPath(DotNetTasks.DotNetPath));
                });
            });

        protected Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                Repository.Artifacts.ForEach(artifact =>
                {
                    artifact.Initialize(Repository);

                    DotNetBuild(s => s
                        .SetProjectFile(artifact.Path)
                        .SetConfiguration(Configuration)
                        .Execute(settings => VersioningStrategy.SetDotNetBuildVersion(settings, this, artifact))
                          
                        .SetProcessToolPath(DotNetTasks.DotNetPath));
                });
            });

        protected Target Pack => _ => _
            .Executes(() =>
            {
                Repository.Artifacts.Where(a => a.Type == ArtifactType.Package).ForEach(artifact =>
                {
                    artifact.Initialize(Repository);

                    // if (_repository.UseGitVersion)
                    // {
                    //     Serilog.Log.Information(
                    //         $"gitVersion: {_repository.UseGitVersion}, {GitVersion.NuGetVersionV2} / {GitVersion.AssemblySemFileVer} / {GitVersion.AssemblySemVer}");
                    // }

                    DotNetPack(s => s
                        .SetProject(artifact.Path)
                        .SetConfiguration(Configuration)
                        .Execute(settings => VersioningStrategy.SetDotNetPackVersion(settings, this, artifact))
                        // .When(_repository.UseGitVersion, (settings) => settings
                        //     .SetVersion(GitVersion.NuGetVersionV2)
                        //     .SetFileVersion(GitVersion.AssemblySemFileVer)
                        //     .SetAssemblyVersion(GitVersion.AssemblySemVer))                        
                        .SetOutputDirectory(Repository.PackagesDirectory / artifact.Name)
                        //.EnableNoBuild()
                        .SetProcessToolPath(DotNetTasks.DotNetPath));
                });
            });

        protected Target Push => _ => _
            .Requires(() => NugetApiUrl)
            .Requires(() => NugetApiKey)
            .Executes(() =>
            {
                Serilog.Log.Information($"pushing to: {NugetApiUrl}");

                Repository.Artifacts.Where(a => a.Type == ArtifactType.Package).ForEach(artifact =>
                {
                    artifact.Initialize(Repository);

                    DotNetNuGetPush(s => s
                        .Execute(settings => VersioningStrategy.SetDotNetNuGetPushVersion(settings, this, artifact))
                        // .When(_repository.UseGitVersion, (settings) => settings
                        //     .SetTargetPath(_repository.PackagesDirectory / artifact.Name /
                        //                    $"{artifact.Name}.{GitVersion.NuGetVersionV2}.nupkg"))                        
                        .SetSource(NugetApiUrl)
                        .SetApiKey(NugetApiKey)
                        .SetSkipDuplicate(true)
                        .SetProcessToolPath(DotNetTasks.DotNetPath)
                    );
                });
            });


        protected Target Publish => _ => _
            .Executes(() =>
            {
                Repository.Artifacts.Where(a => a.Type == ArtifactType.Service).ForEach(artifact =>
                {
                    artifact.Initialize(Repository);

                    DotNetPublish(s => s
                        .SetProject(artifact.Path)
                        .SetConfiguration(Configuration)
                        // .When(_repository.UseGitVersion, (settings) => settings
                        //     .SetVersion(GitVersion.NuGetVersionV2)
                        //     .SetFileVersion(GitVersion.AssemblySemFileVer)
                        //     .SetAssemblyVersion(GitVersion.AssemblySemVer))
                        .SetOutput(Repository.ServicesDirectory / artifact.Name)
                        .EnableNoBuild()
                        .SetProcessToolPath(DotNetTasks.DotNetPath));
                });
            });

        protected Target UnitTests => _ => _
            .Executes(() =>
            {
                Repository.DetectTests();
                Repository.Tests.Where(test => test.Type == TestType.UnitTests).ForEach(test =>
                {
                    DotNetTest(s => ConfigureTestSettings(s, test,
                        test.Equals(Repository.Tests.Last(t => t.Type == test.Type))));
                });
            });

        protected Target IntegrationTests => _ => _
            .Executes(() =>
            {
                Repository.DetectTests();
                Repository.Tests.Where(test => test.Type == TestType.IntegrationTests).ForEach(test =>
                {
                    DotNetTest(s => ConfigureTestSettings(s, test,
                        test.Equals(Repository.Tests.Last(t => t.Type == test.Type))));
                });
            });

        private DotNetTestSettings ConfigureTestSettings(DotNetTestSettings settings, Test test, bool isFinal = false)
        {
            return settings
                .SetProjectFile(test.Project)
                .SetLoggers($"trx;LogFileName={test.Project.NameWithoutExtension}.trx")
                .SetConfiguration(Configuration)
                .SetResultsDirectory(Repository.TestResultsDirectory)
                .SetProcessToolPath(DotNetTasks.DotNetPath)
                .When(Constants.TestCategories.CodeCoverageCategories.Contains(test.Type), x =>
                    x.SetProcessArgumentConfigurator(args =>
                        args
                            .Add("/p:CollectCoverage=true")
                            .Add("/maxcpucount:1")
                            .Add($"/p:MergeWith={Repository.TestCoverageDirectory}/coverage.temp.json")
                            .Add($"/p:CoverletOutput={Repository.TestCoverageDirectory}/coverage.temp.json", !isFinal)
                            .Add($"/p:CoverletOutput={Repository.TestCoverageDirectory}/coverage.xml", isFinal)
                            .Add("/p:CoverletOutputFormat=cobertura", isFinal)));
        }
    }
}