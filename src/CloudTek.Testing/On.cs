namespace CloudTek.Testing;

/// <summary>
/// An enumeration of possible test execution operating systems
/// </summary>
[Flags]
public enum On
{
  /// <summary>
  /// All operating systems
  /// </summary>
  All = 0,

  /// <summary>
  /// Windows only
  /// </summary>
  Windows = 1,

  /// <summary>
  /// Linux only
  /// </summary>
  Linux = 2,

  /// <summary>
  /// MacOS only
  /// </summary>
  MacOS = 4
}