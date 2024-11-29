using CloudTek.Build.Internals;
using FluentAssertions;
using Nuke.Common.IO;

namespace CloudTek.Build.Tests;

// ReSharper disable once ClassNeverInstantiated.Global
public class EmbeddedResourceExtensionsTests
{
  private static readonly AbsolutePath Temp = AbsolutePath.Create(Directory.GetCurrentDirectory()) / "tmp";
  public EmbeddedResourceExtensionsTests()
  {
    if (Directory.Exists(Temp))
    {
      Directory.Delete(Temp, true);
    }
  }
  public class GivenGetEmbeddedResources
  {
    [Fact]
    public void WhenFullyQualifiedResourceName_ThenEmbeddedResourcePrefixesAreReturned()
    {
      // Arrange
      const string prefix = "CloudTek.Build..templates..husky";

      // Act
      var resources = typeof(AssemblyInfo).Assembly.GetEmbeddedResources(prefix);

      // Assert
      resources.Should().NotBeEmpty();
    }

    [Fact]
    public void WhenPartialPrefix_ThenEmbeddedResourcePrefixesAreReturned()
    {
      // Arrange
      const string prefix = ".templates..husky";

      // Act
      var resources = typeof(AssemblyInfo).Assembly.GetEmbeddedResources(prefix);

      // Assert
      resources.Should().NotBeEmpty();
    }
  }

  public class GivenCopyResource
  {
    [Fact]
    public async Task WhenResourceExists_ThenResourceIsCopied()
    {
      // Arrange
      const string resource = ".templates..husky.task-runner.json";

      // Act
      var file = await typeof(AssemblyInfo).Assembly.CopyResource(
        resource: resource,
        path: Temp / nameof(WhenResourceExists_ThenResourceIsCopied).ToLowerInvariant());

      // Assert
      File.Exists(file).Should().BeTrue();
    }

    [Fact]
    public async Task WhenResourceDoesNotExists_ThenInvalidOperationExceptionIsThrown()
    {
      // Arrange
      const string resource = ".templates..husky.non-existing-file.json";

      // Act & Assert
      var action = async () =>
       await typeof(AssemblyInfo).Assembly.CopyResource(
        resource: resource,
        path: Temp / nameof(WhenResourceExists_ThenResourceIsCopied).ToLowerInvariant());

      await action.Should().ThrowAsync<InvalidOperationException>();
    }
  }

  public class GivenCopyResources
  {
    [Fact]
    public async Task WhenResourcesExist_ThenResourcesAreCopied()
    {
      // Arrange
      const string prefix = ".templates..husky";

      // Act
      var files = (await typeof(AssemblyInfo).Assembly.CopyResources(
        prefix: prefix,
        path: Temp / nameof(WhenResourcesExist_ThenResourcesAreCopied).ToLowerInvariant())).ToArray();

      // Assert
      files.Should().NotBeNullOrEmpty();
      files.Length.Should().Be(4);

      foreach (var file in files)
      {
        var fileInfo = new FileInfo(file);
        fileInfo.Exists.Should().BeTrue();
        fileInfo.Length.Should().BeGreaterThan(0);
      }
    }
  }
}