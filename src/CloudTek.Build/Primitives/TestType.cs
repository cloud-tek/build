namespace CloudTek.Build.Primitives;

/// <summary>
/// An enumeration describing possible types of tests executed by the SmartBuild
/// </summary>
public enum TestType
{
  /// <summary>
  /// UnitTests
  /// </summary>
  UnitTests = 0,

  /// <summary>
  /// IntegrationTests
  /// </summary>
  IntegrationTests,

  /// <summary>
  /// ModuleTests (acceptance tests against a module)
  /// </summary>
  ModuleTests,

  /// <summary>
  /// EndToEndTests (acceptance tests against a system)
  /// </summary>
  E2ETests
}