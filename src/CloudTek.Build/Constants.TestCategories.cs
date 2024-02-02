using CloudTek.Build.Primitives;

namespace CloudTek.Build;

/// <summary>
/// Constants used by the SmartBuild
/// </summary>
public static class Constants
{
  /// <summary>
  /// Helper class for test execution
  /// </summary>
  public static class TestCategories
  {
#pragma warning disable CA2211, MA0069
    /// <summary>
    /// Tests belonging to CodeCoverageCategories will trigger code coverage analysis
    /// </summary>
    public static TestType[] CodeCoverageCategories =
    {
      TestType.UnitTests,
      TestType.IntegrationTests
    };
#pragma warning restore CA2211, MA0069
  }
}