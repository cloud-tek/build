using CloudTek.Testing;
using FluentAssertions;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using Xunit;
using Xunit.Abstractions;

namespace CloudTek.Build.Tests;

public class RepositoryTests : BaseFixture, IClassFixture<TestBuildFixture>
{
  private readonly TestBuildFixture _fixture;

  public RepositoryTests(TestBuildFixture fixture, ITestOutputHelper output)
    : base(output)
  {
    _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
  }

  [UnitTest]
  [SmartFact(Execute.Always)]
  public void GivenSmartBuild_WhenRepositoryArtifactsInitialized_ThenArtifactsProjectsAreResolved()
  {
    _fixture.Repository.Artifacts.ForEach(
      a =>
      {
        a.Projects.Should().NotBeNullOrEmpty();
        a.Projects.ForEach(
          project =>
          {
            project.Path.Should().NotBeNull();
            project.Path.Exists("file").Should().BeTrue();
          });
      });
  }

  [UnitTest]
  [SmartFact(Execute.Always)]
  public void GivenSmartBuild_WhenInitialized_ThenTestProjectsAreResolved()
  {
    _fixture.Repository.Tests.ForEach(
      a =>
      {
        a.Project.Should().NotBeNull();
        a.Project.Exists("file").Should().BeTrue();
      });
  }
}