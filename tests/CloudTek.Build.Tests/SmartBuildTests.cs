using CloudTek.Build.Packaging;
using CloudTek.Build.Versioning;
using CloudTek.Testing;
using FluentAssertions;

namespace CloudTek.Build.Tests;

public class SmartBuildTests : SmartBuild<VersioningStrategy.Default>
{
  [UnitTest]
  [Fact]
  public void X()
  {
    Execute<SmartBuildTests>(x => x.Clean)
      .Should().Be(0);
  }
}