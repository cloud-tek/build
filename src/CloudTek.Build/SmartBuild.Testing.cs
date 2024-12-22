using CloudTek.Build.Extensions;
using CloudTek.Build.Primitives;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using ToolSettingsExtensions = CloudTek.Build.Extensions.ToolSettingsExtensions;

namespace CloudTek.Build
{
  public abstract partial class SmartBuild
  {
    private bool _coverletAdded;

    /// <summary>
    /// dotnet nuke --target Test
    /// Executes Unit and Integration tests
    /// </summary>
    protected virtual Target Test => _ => _
      .Description("Run all tests (Unit and Integration)")
      .DependsOn(UnitTests, IntegrationTests, RunChecks)
      .Executes(() => { });

    /// <summary>
    /// Initializes the code-coverage collector
    /// </summary>
    protected virtual Target InitializeCoverageCollector =>
      _ => _.Executes(InitializeCoverletCollector);

    /// <summary>
    /// dotnet nuke --target UnitTests
    /// </summary>
    protected virtual Target UnitTests => _ => _
      .Before(IntegrationTests)
      .DependsOn(Compile, InitializeCoverageCollector)
      .Executes(
        () =>
        {
          DotNetTest(s => ConfigureTestSettings(s, TestType.UnitTests));
        });

    /// <summary>
    /// dotnet nuke --target IntegrationTests
    /// </summary>
    protected virtual Target IntegrationTests => _ => _
      .DependsOn(Compile, InitializeCoverageCollector)
      .Executes(
        () =>
        {
          DotNetTest(s => ConfigureTestSettings(s, TestType.IntegrationTests));
        });

    private DotNetTestSettings ConfigureTestSettings(DotNetTestSettings settings, TestType type)
    {
      return settings
        .SetLoggers($"trx")
        .SetConfiguration(Configuration)
        .SetResultsDirectory(Repository.TestResultsDirectory)
        .SetProcessToolPath(DotNetPath)
        .SetNoRestore(SolutionRestored)
        .SetNoBuild(SolutionBuilt)
        .Execute(s => s.SetProcessEnvironmentVariables(EnvironmentVariables))
        .ExecuteWhen(predicate: CollectCoverage, action: s =>
          s.SetDataCollector(
            "XPlat Code Coverage;Format=Cobertura;IncludeTestAssembly=false;ExcludeAssembliesWithoutSources=MissingAll;ExcludeByFile=**/*.g.cs;Exclude=[*]*Migrations*;"))
        .SetFilter(TestFilter)
        .SetFilter($"Category={type}");
    }

    private void InitializeCoverletCollector()
    {
      if (CollectCoverage && !_coverletAdded)
      {
        Log.Information("Initializing coverlet.collector package in all test projects ...");

        Repository.Projects.Where(p => p.Type == ProjectType.Test && p.ProjectProperties.HasCodeCoveragePackage != true)
          .ForEach(
            p =>
            {
              DotNet($"add {p.Path} package coverlet.collector", Repository.RootDirectory);
            });

        _coverletAdded = true;
      }
    }
  }
}