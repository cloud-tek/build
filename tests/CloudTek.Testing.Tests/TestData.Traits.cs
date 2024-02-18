using Xunit;

namespace CloudTek.Testing.Tests;

public partial class TestData
{
  /// <summary>
  /// Test data for the <see cref="TraitTests"/> class. This class does not contain actual test cases.
  /// </summary>
  public class Traits
  {
    [Fact(Skip = nameof(TestData))]
    [UnitTest]
    public void UnitTest()
    {
    }

    [Fact(Skip = nameof(TestData))]
    [IntegrationTest]
    public void IntegrationTest()
    {
    }

    [Fact(Skip = nameof(TestData))]
    [ModuleTest]
    public void ModuleTest()
    {
    }

    [Fact(Skip = nameof(TestData))]
    [SystemTest]
    public void SystemTest()
    {
    }

    [Fact(Skip = nameof(TestData))]
    [SmokeTest]
    public void SmokeTest()
    {
    }
  }
}