using CloudTek.Build.Internals;
using CloudTek.Testing;
using FluentAssertions;
using Nuke.Common.IO;

namespace CloudTek.Build.Tests;

public sealed class ToolManifestLookupTests : IDisposable
{
  private const string Listing = """{ "version": 1, "isRoot": true, "tools": { "husky": { "version": "0.7.2", "commands": ["husky"] } } }""";
  private const string Empty = """{ "version": 1, "isRoot": true, "tools": {} }""";

  private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

  public ToolManifestLookupTests()
  {
    Directory.CreateDirectory(root);
  }

  public void Dispose()
  {
    if (Directory.Exists(root))
    {
      Directory.Delete(root, true);
    }
  }

  [IntegrationTest]
  [Fact]
  public void ContainsTool_when_config_manifest_lists_package_returns_true()
  {
    // Arrange
    Directory.CreateDirectory(Path.Combine(root, ".config"));
    File.WriteAllText(Path.Combine(root, ".config", "dotnet-tools.json"), Listing);

    // Act
    var result = ToolManifest.ContainsTool(AbsolutePath.Create(root), "husky");

    // Assert
    result.Should().BeTrue();
  }

  [IntegrationTest]
  [Fact]
  public void ContainsTool_when_root_level_manifest_lists_package_returns_true()
  {
    // Arrange
    File.WriteAllText(Path.Combine(root, "dotnet-tools.json"), Listing);

    // Act
    var result = ToolManifest.ContainsTool(AbsolutePath.Create(root), "husky");

    // Assert
    result.Should().BeTrue();
  }

  [IntegrationTest]
  [Fact]
  public void ContainsTool_when_parent_directory_manifest_lists_package_returns_true()
  {
    // Arrange
    var start = Path.Combine(root, "a", "b");
    Directory.CreateDirectory(start);
    File.WriteAllText(Path.Combine(root, "dotnet-tools.json"), Listing);

    // Act
    var result = ToolManifest.ContainsTool(AbsolutePath.Create(start), "husky");

    // Assert
    result.Should().BeTrue();
  }

  [IntegrationTest]
  [Fact]
  public void ContainsTool_when_root_manifest_lacks_package_returns_false()
  {
    // Arrange
    File.WriteAllText(Path.Combine(root, "dotnet-tools.json"), Empty);

    // Act
    var result = ToolManifest.ContainsTool(AbsolutePath.Create(root), "husky");

    // Assert
    result.Should().BeFalse();
  }
}