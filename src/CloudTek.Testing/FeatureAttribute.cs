namespace CloudTek.Testing;

/// <summary>
/// Attribute used to decorate tests, creating a mapping to requirements being tested
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class FeatureAttribute(string id) : Attribute
{
  public string Id { get; init; } = id;
}