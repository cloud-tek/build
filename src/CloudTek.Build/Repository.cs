using System;
using System.Collections.Generic;
using System.Linq;
using CloudTek.Build.Primitives;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;

namespace CloudTek.Build
{
    public enum RepositoryMode
    {
        SingleModule = 0,
        MultiModule = 1
    }
    public class Repository
    {
        public AbsolutePath RootDirectory { get; private set; } = default!;

        public RepositoryMode Mode { get; set; }
        public virtual AbsolutePath SourceDirectory => RootDirectory / "src";
        public virtual AbsolutePath TestsDirectory => RootDirectory / "tests";
        public virtual AbsolutePath DemoDirectory => RootDirectory / "demo";
        public virtual AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
        public virtual AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";
        public virtual AbsolutePath ServicesDirectory => ArtifactsDirectory / "services";

        public AbsolutePath TestResultsDirectory => TestsDirectory / "results";
        public AbsolutePath TestCoverageDirectory => TestsDirectory / "coverage";
        public bool UseGitVersion { get; } = false; // disabled until valid pipeline tagging exists
        public Artifact[] Artifacts { get; set; } = default!;

        public Test[] Tests { get; private set; } = default!;

        public void Initialize(AbsolutePath rootDirectory)
        {
            RootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));

            if (Artifacts.Length == 0)
            {
                throw new SmartBuildException(SmartBuildError.NoArtifacts);
            }

            if (Artifacts.All(a => string.IsNullOrEmpty(a.Module)))
            {
                Mode = RepositoryMode.SingleModule;
                return;
            }

            if (Artifacts.All(a => !string.IsNullOrEmpty(a.Module)))
            {
                Mode = RepositoryMode.MultiModule;
                return;
            }

            throw new SmartBuildException(SmartBuildError.MixedModules);
        }

        public bool ShouldEmitBetaPackage(GitRepository gitRepository)
        {
            return !(gitRepository.IsOnMasterBranch() || gitRepository.IsOnMainBranch());
        }

        public bool ShouldEmitPackage(GitRepository gitRepository)
        {
            return gitRepository.IsOnMasterBranch() || gitRepository.IsOnMainBranch();
        }

        public virtual void DetectTests()
        {
            var dirs = Mode == RepositoryMode.SingleModule
                ? TestsDirectory.GlobDirectories("*Test*")
                : Artifacts.Select(a => a.Module).SelectMany(module => (RootDirectory/module/"tests").GlobDirectories("*Test*"));

            var result = new List<Test>();

            dirs.ForEach(dir =>
            {
                var project = dir.GlobFiles("*.*sproj").FirstOrDefault();

                if (project != default(AbsolutePath))
                {
                    result.Add(new Test()
                    {
                        Project = dir.GlobFiles("*.*sproj").First(),
                        Type = dir.Name.Contains("Integration") ? TestType.IntegrationTests : TestType.UnitTests
                    });
                }
                else
                {
                    Serilog.Log.Error($"{dir.Name} does not contain a project file");
                }
            });

            Tests = result.ToArray();
        }
    }
}
