using Xunit;

namespace CloudTek.Testing;

/// <summary>
///   SmartTheory attribute used to mark a test as a smart XUnit theory
/// </summary>
public sealed class SmartTheoryAttribute : TheoryAttribute
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="SmartTheoryAttribute" /> class.
  /// </summary>
  /// <param name="on"></param>
  public SmartTheoryAttribute(On on)
  {
    Skip = TestExecutionResolver.Resolve(on);
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="SmartTheoryAttribute" /> class.
  /// </summary>
  /// <param name="execute"></param>
  public SmartTheoryAttribute(Execute execute)
  {
    Skip = TestExecutionResolver.Resolve(execute);
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="SmartTheoryAttribute" /> class.
  /// </summary>
  /// <param name="execute"></param>
  /// <param name="on"></param>
  public SmartTheoryAttribute(Execute execute, On on)
  {
    Skip = TestExecutionResolver.Resolve(execute, on);
  }
}