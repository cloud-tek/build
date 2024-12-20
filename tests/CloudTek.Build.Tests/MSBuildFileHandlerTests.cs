using CloudTek.Build.Utilities;
using CloudTek.Testing;
using FluentAssertions;
using Nuke.Common.IO;

namespace CloudTek.Build.Tests;

public class MSBuildFileHandlerTests
{
  [Theory]
  [InlineData("Test1.props.xml", true, false)]
  [InlineData("Test2.props.xml", false, true)]
  [UnitTest]
  public void GivenGetDocument_WhenDocumentDoesNotExist_ThenItIsCreatedOrThrows(string file, bool createIfNotExists, bool shouldThrow)
  {
    // Arrange
    var path = AbsolutePath.Create(Directory.GetCurrentDirectory()) / "tmp" / file;
    if (path.FileExists())
    {
      path.DeleteFile();
    }

    // Act
    var action = () => MsBuildFileHandler.GetDocument(path: path, createIfNotExists: createIfNotExists);

    // Assert
    if (shouldThrow)
    {
      action.Should().Throw<FileNotFoundException>();
    }
    else
    {
      var doc = action();

      doc.Should().NotBeNull();
      doc.Root.Should().NotBeNull();
      doc.Root!.Name.LocalName.Should().Be("Project");
    }
  }

  [Theory]
  [InlineData("Directory.Build01.props.xml", false)]
  [InlineData("Directory.Build02.props.xml", false)]
  [InlineData("Directory.Build03.props.xml", true)]
  [UnitTest]
  public void GivenGetIsPackable_WhenParsingATestFile_ThenValueAsExpected(string file, bool expected)
  {
    // Arrange
    var path = AbsolutePath.Create(Directory.GetCurrentDirectory()) / "TestData" / file;
    path.FileExists().Should().BeTrue();

    // Act
    var value = MsBuildFileHandler.GetIsPackable(path);

    // Assert
    value.Should().Be(expected);
  }

  [Theory]
  [InlineData("Directory.Build01.props.xml", true)]
  [InlineData("Directory.Build02.props.xml", true)]
  [InlineData("Directory.Build03.props.xml", false)]
  [UnitTest]
  public void GivenSetIsPackable_WhenParsingATestFile_ThenValueAsExpected(string file, bool expected)
  {
    // Arrange
    var path = AbsolutePath.Create(Directory.GetCurrentDirectory()) / "TestData" / file;
    var output = AbsolutePath.Create(Directory.GetCurrentDirectory()) / "tmp" / file;

    if (!output.Parent.DirectoryExists())
    {
      output.Parent.CreateDirectory();
    }
    if (output.FileExists())
    {
      output.DeleteFile();
    }

    path.FileExists().Should().BeTrue();

    // Act
    MsBuildFileHandler.SetIsPackable(path: path, value: expected, output: output);

    // Assert
    MsBuildFileHandler.GetIsPackable(output).Should().Be(expected);
  }
}