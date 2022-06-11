using CloudTek.Build.Versioning;
using FluentAssertions;
using Xunit;

namespace CloudTek.Build.Tests.Versioning
{
    public class AssemblyVersionHandlerTests
    {
        [Theory]
        [InlineData("TestData01_Version.csproj", "1.0.1")]
        [InlineData("TestData02_VersionPrefix.csproj", "1.0.2")]
        [InlineData("TestData03_TargetsVersion.csproj", "1.0.3")]
        [InlineData("TestData04_TargetsVersionPrefix.csproj", "1.0.4")]
        public void GivenCsProjectWithVersion_WhenAssemvlyHandlerHandle_ThenVersion(string file, string expectedVersion)
        {
            var version = AssemblyVersionHandler.Handle($"./Versioning/{file}");

            version.Should().Be(expectedVersion);
        }
    }
}