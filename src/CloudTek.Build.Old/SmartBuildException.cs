namespace CloudTek.Build;

/// <summary>
/// Enumeration of errors that can be thrown by SmartBuild
/// </summary>
public enum SmartBuildError
{
  /// <summary>
  /// No artifacts specified in the SmartBuild
  /// </summary>
  NoArtifacts = 0,

  /// <summary>
  /// An artifact's .Path has not been set
  /// </summary>
  NoArtifactPath,
}

/// <summary>
/// Exception thrown by SmartBuild
/// </summary>
public class SmartBuildException : Exception
{
  internal static IDictionary<SmartBuildError, string> Messages = new Dictionary<SmartBuildError, string>
  {
    { SmartBuildError.NoArtifacts, "The repository needs to define at least 1 artifact" },
    { SmartBuildError.NoArtifactPath, "An artifact's .Path has not been set" }
  };

  /// <summary>
  /// Constructor for SmartBuildException
  /// </summary>
  /// <param name="error"></param>
  public SmartBuildException(SmartBuildError error) : base(Messages[error])
  {
  }

  /// <summary>
  /// Error that caused the exception
  /// </summary>
  public SmartBuildError Error { get; init; }
}