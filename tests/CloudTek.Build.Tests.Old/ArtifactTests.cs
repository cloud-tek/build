using CloudTek.Build.Primitives;
using CloudTek.Testing;
using FluentAssertions;
using Nuke.Common.IO;

namespace CloudTek.Build.Tests;

public class ArtifactTests
{
  [UnitTest]
  [SmartFact(Execute.Always)]
  public void GivenArtifactWithValidPath_WhenInitialize_ThenProjectsAreResolved()
  {
    var root = TestHelpers.GetRepositoryRoot(AppDomain.CurrentDomain.BaseDirectory);
    var artifact = new Artifact()
    {
      Type = ArtifactType.Package,
      Path = root / "src" / "CloudTek.Build" / "*.csproj"
    };

    artifact.Initialize();

    artifact.Projects.Should().NotBeNullOrEmpty();
    artifact.Type.Should().Be(ArtifactType.Package);
  }
}