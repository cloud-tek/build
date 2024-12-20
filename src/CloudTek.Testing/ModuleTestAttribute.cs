using Xunit.Sdk;

namespace CloudTek.Testing;

/// <summary>
///   Attribute used to mark a test as a module acceptance test.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
[TraitDiscoverer(ModuleTestDiscoverer.TypeName, AssemblyInfo.Name)]
// ReSharper disable once ClassNeverInstantiated.Global
public class ModuleTestAttribute : Attribute, ITraitAttribute
{
  /// <summary>
  ///   The category passed to the dotnet test's '--filter' argument
  /// </summary>
  public const string Category = "ModuleTests";
}