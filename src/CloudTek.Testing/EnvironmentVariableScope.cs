namespace CloudTek.Testing;

/// <summary>
///   An IDisposable Environment Variable Scope, in which an environment variable is set to a value.
/// </summary>
public class EnvironmentVariableScope : IDisposable
{
  private readonly string _name;

  private EnvironmentVariableScope(string name, string value)
  {
    _name = name ?? throw new ArgumentNullException(nameof(name));

    Environment.SetEnvironmentVariable(_name, value, EnvironmentVariableTarget.Process);
  }

  /// <summary>
  ///   Disposes the scope
  /// </summary>
  public void Dispose()
  {
    Environment.SetEnvironmentVariable(_name, null, EnvironmentVariableTarget.Process);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  ///   Creates a new environment variable scope
  /// </summary>
  /// <param name="name"></param>
  /// <param name="value"></param>
  /// <returns>The scope</returns>
  public static IDisposable Create(string name, string value)
  {
    return new EnvironmentVariableScope(name, value);
  }
}