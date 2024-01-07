namespace CloudTek.Testing;

/// <summary>
/// SmartFact attribute used to mark a test as a smart XUnit fact
/// </summary>
public sealed class SmartFactAttribute : Xunit.FactAttribute
{
  /// <summary>
  /// Initializes a new instance of the <see cref="SmartFactAttribute"/> class.
  /// </summary>
  /// <param name="on"></param>
  public SmartFactAttribute(On on)
  {
    Skip = TestExecutionResolver.Resolve(on);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SmartFactAttribute"/> class.
  /// </summary>
  /// <param name="execute"></param>
  public SmartFactAttribute(Execute execute)
  {
    Skip = TestExecutionResolver.Resolve(execute);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SmartFactAttribute"/> class.
  /// </summary>
  /// <param name="execute"></param>
  /// <param name="on"></param>
  public SmartFactAttribute(Execute execute, On on)
  {
    Skip = TestExecutionResolver.Resolve(execute, on);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SmartFactAttribute"/> class.
  /// </summary>
  /// <param name="execute"></param>
  /// <param name="on"></param>
  /// <param name="environment"></param>
  public SmartFactAttribute(Execute execute, On on, params string[] environment)
  {
    Skip = TestExecutionResolver.Resolve(execute, on, environment);
  }
}