using CloudTek.Build.Primitives;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;

namespace CloudTek.Build.Tests;

public class TestBuildFixture : IDisposable
{
  // ReSharper disable once MemberCanBePrivate.Global
  public TestBuild SUT { get; private set; }
  // ReSharper disable once MemberCanBePrivate.Global
  public Repository Repository => ((SmartBuild)SUT).Repository;

  /// <summary>
  /// Initializes the TestBuild since normal startup cycle is not accessible
  /// </summary>
  public TestBuildFixture()
  {
    var root = TestHelpers.GetRepositoryRoot(AppDomain.CurrentDomain.BaseDirectory);
    SUT = new TestBuild(() => SolutionModelTasks.CreateSolution(
      solutionFile: root / "*.sln",
      solutions: Enumerable.Empty<Solution>()));

    Repository.DetectTests(SUT);
    Repository.Initialize(root);
    Repository.Artifacts.ForEach(
      a =>
      {
        a.Initialize();
      });
  }

  public void Dispose()
  {
    SUT = null!;
    GC.SuppressFinalize(this);
  }
}