using Xunit.Abstractions;
using Xunit.Sdk;

namespace CloudTek.Testing;

/// <summary>
/// Discoverer for the <see cref="ModuleTestAttribute"/>
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class ModuleTestDiscoverer : ITraitDiscoverer
{
  /// <summary>
  /// The fully qualified type name of this discoverer
  /// </summary>
  public const string TypeName = AssemblyInfo.Name + "." + nameof(ModuleTestDiscoverer);

  /// <summary>
  /// Gets the trait values from the <paramref name="traitAttribute"/>
  /// </summary>
  /// <param name="traitAttribute"></param>
  /// <returns>Traits of the target attribute</returns>
  public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
  {
    yield return new KeyValuePair<string, string>("Category", ModuleTestAttribute.Category);
  }
}