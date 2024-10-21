using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

namespace CloudTek.Build
{
  /// <summary>
  /// Represents a repository containing the code, tests and artifacts
  /// </summary>
  public class Repository
  {
    /// <summary>
    /// The root execution directory
    /// </summary>
    public AbsolutePath RootDirectory { get; } = NukeBuild.RootDirectory;
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
    /// The projects in the solution, excluding the build project
    /// </summary>
    public IReadOnlyCollection<Primitives.Project> Projects { get; private set; } = [];

    /// <summary>
    /// The default constructor
    /// </summary>
    /// <param name="solution"></param>
    public Repository(Solution solution)
    {
      Projects = solution.AllProjects
        .Where(p => p.Name != "_build")
        .Select(p => new Primitives.Project(p.Path))
        .ToList();
    }

    // TODO: convert to extension method?
    /// <summary>
    /// Checks if the artifacts from current version should append a sem-ver beta suffix
    /// </summary>
    /// <param name="gitRepository"></param>
    /// <returns></returns>
#pragma warning disable CA1822
    public bool ShouldAddBetaSuffix(GitRepository? gitRepository)
#pragma warning restore CA1822
    {
      return gitRepository != null && !(gitRepository.IsOnMasterBranch() || gitRepository.IsOnMainBranch());
    }
  }
}