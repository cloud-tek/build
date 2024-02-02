namespace CloudTek.Testing;

/// <summary>
/// An enumeration of possible execution environments for a Smart test
/// </summary>
[Flags]
public enum Execute
{
  /// <summary>
  /// Test is always executed
  /// </summary>
  Always = 0,

  /// <summary>
  /// Test is executed only in GitHub Actions CI
  /// </summary>
  InGithubActions = 1,

  /// <summary>
  /// Test is executed only in Azure DevOps CI
  /// </summary>
  InAzureDevOps = 2,

  /// <summary>
  /// Test is executed only in a container
  /// </summary>
  InContainer = 4,

  /// <summary>
  /// Test is executed only in DEBUG configuration
  /// </summary>
  InDebug = 8
}