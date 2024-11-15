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
      _ => _.Executes(InitializeCoverageCollectorMethod);

    /// <summary>
    /// dotnet nuke --target UnitTests
    /// </summary>
    protected virtual Target UnitTests => _ => _
      .Before(IntegrationTests)
      .DependsOn(Compile, InitializeCoverageCollector)
      .Executes(
        () =>
        {
          DotNetTest(s => ConfigureTestSettings(s, false));
        });

    /// <summary>
    /// dotnet nuke --target IntegrationTests
    /// </summary>
    protected virtual Target IntegrationTests => _ => _
      .DependsOn(Compile, InitializeCoverageCollector)
      .Executes(
        () =>
        {
          DotNetTest(s => ConfigureTestSettings(s, true));
        });

    private DotNetTestSettings ConfigureTestSettings(DotNetTestSettings settings, bool isIntegrationTests)
    {
      return ToolSettingsExtensions.When(
          settings
            .SetLoggers($"trx")
            .SetConfiguration(Configuration)
            .SetResultsDirectory(Repository.TestResultsDirectory)
            .SetProcessToolPath(DotNetPath)
            .SetRuntime(Runtime)
            .SetNoRestore(SolutionRestored)
            .SetNoBuild(SolutionBuilt)
            .Execute(s => s.SetProcessEnvironmentVariables(EnvironmentVariables)),
          CollectCoverage,
          s => s.SetDataCollector(
            "XPlat Code Coverage;Format=Cobertura;IncludeTestAssembly=false;ExcludeAssembliesWithoutSources=MissingAll;ExcludeByFile=**/*.g.cs;Exclude=[*]*Migrations*;"))
        .SetFilter(
          (string.IsNullOrWhiteSpace(TestFilter) ? "" : TestFilter + "&") + (isIntegrationTests
            ? "FullyQualifiedName~Integration"
            : "FullyQualifiedName!~Integration"));
    }

    private void InitializeCoverageCollectorMethod()
    {
      if (CollectCoverage && !_coverletAdded)
      {
        Log.Information("Initializing coverlet.collector package for coverage collection");

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