namespace CloudTek.Git;

/// <summary>
/// An enumeration of possible commit message errors
/// </summary>
public enum CommitMessageAnalysisResult
{
  /// <summary>
  /// No error. Message is Valid. (default)
  /// </summary>
  Ok = 0,

  /// <summary>
  /// No commit message was provided
  /// </summary>
  Empty,

  /// <summary>
  /// Commit message is invalid
  /// </summary>
  Invalid,
}