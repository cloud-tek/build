#pragma warning disable CA2211,MA0069
using System.ComponentModel;
using Nuke.Common.Tooling;

namespace CloudTek.Build;

/// <summary>
/// An enumeration of possible build configurations
/// </summary>
[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
  /// <summary>
  /// Debug build configuration
  /// </summary>
  public static Configuration Debug = new() { Value = nameof(Debug) };

  /// <summary>
  /// Release build configuration
  /// </summary>
  public static Configuration Release = new() { Value = nameof(Release) };

  /// <summary>
  ///  Implicit conversion from string to Configuration
  /// </summary>
  /// <param name="configuration"></param>
  public static implicit operator string(Configuration configuration)
  {
    return configuration.Value;
  }
}
#pragma warning restore CA2211, MA0069