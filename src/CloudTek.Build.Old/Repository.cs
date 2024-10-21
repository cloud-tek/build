using CloudTek.Build.Primitives;
using Nuke.Common.Git;
using Nuke.Common.IO;

namespace CloudTek.Build;

/// <summary>
/// Represents a repository containing the code, tests and artifacts
/// </summary>
public class Repository
{
  /// <summary>
  /// The root directory of the repository
  /// </summary>
  public AbsolutePath RootDirectory { get; private set; } = default!;

  /// <summary>
  /// The directory where the source code is stored
  /// </summary>
  public virtual AbsolutePath SourceDirectory => RootDirectory / "src";

  /// <summary>
  /// The directory where the testing results are stored
  /// </summary>
  public virtual AbsolutePath ResultsDirectory => RootDirectory / "results";

  /// <summary>
  /// The directory where demo artifacts are emitted to
  /// </summary>
  public virtual AbsolutePath DemoDirectory => RootDirectory / "demo";

  /// <summary>
  /// The directory where all artifacts are emitted to
  /// </summary>
  public virtual AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

  /// <summary>
  /// The directory where all packages are emitted to
  /// </summary>
  public virtual AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";

  /// <summary>
  /// The directory where all services are emitted to
  /// </summary>
  public virtual AbsolutePath ServicesDirectory => ArtifactsDirectory / "services";

  /// <summary>
  /// The directory where test results are stored
  /// </summary>
  public AbsolutePath TestResultsDirectory => ResultsDirectory / "tests";

  /// <summary>
  /// The directory where test coverage results are stored
  /// </summary>
  public AbsolutePath TestCoverageDirectory => ResultsDirectory / "coverage";

  /// <summary>
  /// An array of artifacts declared in the repository
  /// </summary>
  public Artifact[] Artifacts { get; set; } = default!;

  /// <summary>
  /// An array of test projects detected in the repository
  /// </summary>
  public Test[] Tests { get; private set; } = default!;

  /// <summary>
  /// Initializes the repository
  /// </summary>
  /// <param name="rootDirectory"></param>
  /// <exception cref="ArgumentNullException">Thrown when one of the parameters is null.</exception>
  /// <exception cref="SmartBuildException">Thrown when SmartBuild-specific error occurs</exception>
  public void Initialize(AbsolutePath rootDirectory)
  {
    RootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));

    if (Artifacts.Length == 0)
      throw new SmartBuildException(SmartBuildError.NoArtifacts);
  }

#pragma warning disable CA1822
  /// <summary>
  /// Determines whether a BETA package should be emitted
  /// </summary>
  /// <param name="gitRepository"></param>
  /// <returns>boolean</returns>
  public bool ShouldEmitBetaPackage(GitRepository? gitRepository)
#pragma warning restore CA1822
  {
    return gitRepository != null && !(gitRepository.IsOnMasterBranch() || gitRepository.IsOnMainBranch());
  }

#pragma warning disable CA1822
  /// <summary>
  ///  Determines whether a package should be emitted
  /// </summary>
  /// <param name="gitRepository"></param>
  /// <returns>boolean</returns>
  public bool ShouldEmitPackage(GitRepository? gitRepository)
#pragma warning restore CA1822
  {
    return gitRepository == null || gitRepository.IsOnMasterBranch() || gitRepository.IsOnMainBranch();
  }

  /// <summary>
  /// Detects all test projects in the solution and adds them to Repository.Tests
  /// </summary>
  /// <param name="build"></param>
  public virtual void DetectTests(SmartBuild build)
  {
    Tests = build.Solution.AllProjects.Where(p => p.Name.Contains("Tests")).Select(p => new Test()
    {
      Project = p.Path
    }).ToArray();
  }
}