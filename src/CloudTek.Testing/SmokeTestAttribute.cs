using Xunit.Sdk;

namespace CloudTek.Testing;

/// <summary>
///   Attribute used to mark a test as an smoke test.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[TraitDiscoverer(SmokeTestDiscoverer.TypeName, AssemblyInfo.Name)]
// ReSharper disable once ClassNeverInstantiated.Global
public class SmokeTestAttribute : Attribute, ITraitAttribute
{
  /// <summary>
  ///   The category passed to the dotnet test's '--filter' argument
  /// </summary>
  public const string Category = "SmokeTests";
}