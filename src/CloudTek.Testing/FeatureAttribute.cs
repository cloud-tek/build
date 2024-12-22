namespace CloudTek.Testing;

/// <summary>
/// Attribute used to decorate tests, creating a mapping to requirements being tested
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class FeatureAttribute(string id) : Attribute
{
  /// <summary>
  /// Feature's identitifier in the respective requirements management system
  /// </summary>
  public string Id { get; init; } = id;
}