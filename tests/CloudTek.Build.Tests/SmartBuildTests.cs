using System.Globalization;
using System.Reflection;
using FluentAssertions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace CloudTek.Build.Tests;

public class SmartBuildTests : TestBuildFixture
{
  private readonly ITestOutputHelper _output;
  public SmartBuildTests(ITestOutputHelper output)
  {
    _output = output;

    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Debug()
      .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose, formatProvider: CultureInfo.InvariantCulture)
      .CreateLogger();
  }

  // [Fact]
  // public void GivenSmartBuild_WhenInitialized_ThenArtifactsProjectsAreResolved()
  // {
  //   var sut = InitializeTestBuild();
  //   var repository = ((SmartBuild)sut).Repository;
  //
  //   repository.Artifacts.ForEach(
  //     a =>
  //     {
  //       a.Projects.Should().NotBeNullOrEmpty();
  //       a.Projects.ForEach(
  //         project =>
  //         {
  //           project.Path.Exists("file").Should().BeTrue();
  //         });
  //     });
  // }
  //
  // [Fact]
  // public void GivenSmartBuild_WhenInitialized_ThenTestProjectsAreResolved()
  // {
  //   var sut = InitializeTestBuild();
  //   var repository = ((SmartBuild)sut).Repository;
  //
  //   repository.Tests.ForEach(
  //     a =>
  //     {
  //       a.Project.Should().NotBeNull();
  //       a.Project.Exists("file").Should().BeTrue();
  //     });
  // }
}