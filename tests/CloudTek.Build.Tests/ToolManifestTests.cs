using CloudTek.Build.Internals;
using CloudTek.Testing;
using FluentAssertions;

namespace CloudTek.Build.Tests;

public class ToolManifestTests
{
  private const string Manifest = """{ "version": 1, "isRoot": true, "tools": { "Husky": { "version": "0.7.2", "commands": ["husky"] } } }""";

  [UnitTest]
  [Fact]
  public void ListsTool_when_manifest_contains_package_returns_true()
  {
    // Arrange
    // Act
    var result = ToolManifest.ListsTool(Manifest, "Husky");

    // Assert
    result.Should().BeTrue();
  }

  [UnitTest]
  [Fact]
  public void ListsTool_with_different_casing_returns_true()
  {
    // Arrange
    // Act
    var result = ToolManifest.ListsTool(Manifest, "husky");

    // Assert
    result.Should().BeTrue();
  }

  [UnitTest]
  [Fact]
  public void ListsTool_when_package_absent_returns_false()
  {
    // Arrange
    // Act
    var result = ToolManifest.ListsTool(Manifest, "dotnet-outdated-tool");

    // Assert
    result.Should().BeFalse();
  }

  [UnitTest]
  [Fact]
  public void ListsTool_when_tools_section_missing_returns_false()
  {
    // Arrange
    const string json = """{ "version": 1, "isRoot": true }""";

    // Act
    var result = ToolManifest.ListsTool(json, "Husky");

    // Assert
    result.Should().BeFalse();
  }

  [UnitTest]
  [Fact]
  public void IsRoot_when_isRoot_true_returns_true()
  {
    // Arrange
    // Act
    var result = ToolManifest.IsRoot(Manifest);

    // Assert
    result.Should().BeTrue();
  }

  [UnitTest]
  [Fact]
  public void IsRoot_when_isRoot_missing_returns_false()
  {
    // Arrange
    const string json = """{ "version": 1, "tools": {} }""";

    // Act
    var result = ToolManifest.IsRoot(json);

    // Assert
    result.Should().BeFalse();
  }
}