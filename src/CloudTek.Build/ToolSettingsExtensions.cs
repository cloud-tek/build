using Nuke.Common.Tooling;

namespace CloudTek.Build;

/// <summary>
/// NUKE ToolSettings extensions
/// </summary>
public static class ToolSettingsExtensions
{
  /// <summary>
  /// Executes the action when the predicate is true.
  /// </summary>
  /// <typeparam name="TSettings">The type of the settings.</typeparam>
  /// <param name="settings"></param>
  /// <param name="predicate"></param>
  /// <param name="action"></param>
  /// <returns>The updated settings.</returns>
  /// <exception cref="ArgumentNullException">Thrown when the action parameter is null.</exception>
  public static TSettings When<TSettings>(
    this TSettings settings,
    bool predicate,
    Func<TSettings, TSettings> action)
    where TSettings : ToolSettings
  {
    _ = action ?? throw new ArgumentNullException(nameof(action));

    if (predicate)
      settings = action(settings);

    return settings;
  }

  /// <summary>
  /// Executes the action.
  /// </summary>
  /// <typeparam name="TSettings">The type of the settings.</typeparam>
  /// <param name="settings"></param>
  /// <param name="action"></param>
  /// <returns>The updated settings.</returns>
  /// <exception cref="ArgumentNullException">Thrown when the action parameter is null.</exception>
  public static TSettings Execute<TSettings>(
    this TSettings settings,
    Func<TSettings, TSettings> action)
    where TSettings : ToolSettings
  {
    _ = action ?? throw new ArgumentNullException(nameof(action));

    settings = action(settings);

    return settings;
  }

  /// <summary>
  /// Executes the action when the predicate is true.
  /// </summary>
  /// <typeparam name="TSettings">The type of the settings.</typeparam>
  /// <param name="settings"></param>
  /// <param name="action"></param>
  /// <param name="predicate"></param>
  /// <returns>The updated settings.</returns>
  /// <exception cref="ArgumentNullException">Thrown when the action parameter is null.</exception>
  public static TSettings ExecuteWhen<TSettings>(this TSettings settings, Func<TSettings, TSettings> action, bool predicate)
    where TSettings : ToolSettings
  {
    _ = action ?? throw new ArgumentNullException(nameof(action));

    if (!predicate)
      return settings;

    settings = action(settings);

    return settings;
  }

  /// <summary>
  ///  Sets the environment variable for the process base on the provided values
  /// </summary>
  /// <typeparam name="TSettings">The type of the settings.</typeparam>
  /// <param name="settings">The settings.</param>
  /// <param name="variables">The environment variables.</param>
  /// <returns>The updated settings.</returns>
  public static TSettings SetProcessEnvironmentVariables<TSettings>(
    this TSettings settings,
    IReadOnlyDictionary<string, string> variables)
    where TSettings : ToolSettings
  {
    foreach (var kvp in variables)
    {
      settings = settings.SetProcessEnvironmentVariable(kvp.Key, kvp.Value);
    }

    return settings;
  }
}