namespace CloudTek.Build.Packaging;

/// <summary>
/// An abstraction representing basic information about a NuGet package
/// </summary>
public record Package
{
  /// <summary>
  /// Name of the package
  /// </summary>
  public string Name { get; set; } = default!;

  /// <summary>
  /// Requested version of the package
  /// </summary>
  public string Requested { get; set; } = default!;

  /// <summary>
  /// Resolved version of the package
  /// </summary>
  public string Resolved { get; set; } = default!;

  /// <summary>
  /// Latest version of the package
  /// </summary>
  public string Latest { get; set; } = default!;

  /// <summary>
  /// Flag indicating whether the package is pinned
  /// </summary>
  public bool IsPinned { get; set; } = default!;
}