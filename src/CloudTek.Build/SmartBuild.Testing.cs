using CloudTek.Build.Primitives;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CloudTek.Build;

public abstract partial class SmartBuild
{
  /// <summary>
  ///   dotnet nuke --target UnitTests --skip-unit-tests true
  /// </summary>
  [Parameter]
  public bool SkipUnitTests { get; set; }

  /// <summary>
  ///   dotnet nuke --target IntegrationTests --skip-integration-tests true
  /// </summary>
  [Parameter]
  public bool SkipIntegrationTests { get; set; }

  /// <summary>
  ///   dotnet nuke --target UnitTests --collect-coverage true
  /// </summary>
  [Parameter]
  public bool CollectCoverage { get; set; }

  /// <summary>
  /// dotnet nuke --target Test
  /// Executes all test targets against the solution
  /// </summary>
  protected virtual Target Test => _ => _
    .BaseTarget(nameof(Test), this)
    .DependsOn(UnitTests, IntegrationTests, RunChecks)
    .Executes(() => { });

  /// <summary>
  /// dotnet nuke --target UnitTests
  /// Executes dotnet test --filter Category=UnitTest against all test projects
  /// </summary>
  protected virtual Target UnitTests => _ => _
    .BaseTarget(nameof(UnitTests), this)
    .DependsOn(Compile)
    .Before(IntegrationTests)
    .OnlyWhenDynamic(() => !SkipUnitTests)
    .WhenSkipped(DependencyBehavior.Skip)
    .Executes(() =>
    {
      Repository.Tests.ForEach(test =>
      {
        InitializeCoverageCollector(test);

        DotNetTest(s => ConfigureTestSettings(s, test, TestType.UnitTests, test.Equals(Repository.Tests.Last())));
      });
    });

  /// <summary>
  /// dotnet nuke --target IntegrationTests
  /// Executes dotnet test --filter Category=IntegrationTest against all test projects
  /// </summary>
  protected virtual Target IntegrationTests => _ => _
    .BaseTarget(nameof(IntegrationTests), this)
    .DependsOn(Compile)
    .OnlyWhenDynamic(() => !SkipIntegrationTests)
    .WhenSkipped(DependencyBehavior.Skip)
    .Executes(() =>
    {
      Repository.Tests.ForEach(test =>
      {
        InitializeCoverageCollector(test);

        DotNetTest(s => ConfigureTestSettings(s, test, TestType.IntegrationTests, test.Equals(Repository.Tests.Last())));
      });
    });

  private DotNetTestSettings ConfigureTestSettings(DotNetTestSettings settings, Test test, TestType type, bool isFinal = false)
  {
    return settings
      .SetProjectFile(test.Project)
      .SetLoggers($"trx;LogFileName={test.Project.NameWithoutExtension}.trx")
      .SetConfiguration(Configuration)
      .SetResultsDirectory(Repository.TestResultsDirectory)
      .SetProcessToolPath(DotNetPath)
      .SetNoRestore(SolutionRestored)
      .SetNoBuild(SolutionBuilt)
      .Execute(s => s.SetProcessEnvironmentVariables(EnvironmentVariables))
      .When(Constants.TestCategories.CodeCoverageCategories.Contains(type) && CollectCoverage, settings =>
        settings.SetProcessArgumentConfigurator(args =>
          args
            .Add("/p:CollectCoverage=true")
            .Add("/maxcpucount:1")
            .Add($"/p:MergeWith={Repository.TestCoverageDirectory}/coverage.temp.json")
            .Add($"/p:CoverletOutput={Repository.TestCoverageDirectory}/coverage.temp.json", !isFinal)
            .Add($"/p:CoverletOutput={Repository.TestCoverageDirectory}/coverage.xml", isFinal)
            .Add("/p:CoverletOutputFormat=cobertura", isFinal)))
      .SetFilter(TestFilter)
      .SetFilter($"Category={type}");
  }

  private void InitializeCoverageCollector(Test test)
  {
    if (CollectCoverage)
      DotNet("add package coverlet.msbuild --version 6.0.0", test.Project.Parent);
  }
}