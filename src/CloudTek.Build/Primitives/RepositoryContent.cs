namespace CloudTek.Build.Primitives;

/// <summary>
/// Base abstraction for possible repository contents
/// </summary>
public abstract class RepositoryContent
{
  /// <summary>
  ///   Optional, specifies the source directory root for a set of artifacts and tests
  /// </summary>
  public string Module { get; set; } = default!;
}