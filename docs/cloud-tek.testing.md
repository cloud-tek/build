# CloudTek.Testing

CloudTek.Testing is a NuGet package extending NUnit to easily integrate with [CloudTek.Build]().


## Test Categories

CloudTek.Build uses the following test categories, which are passed to `dotnet test --filter Category={category}` filter argument. This allows to reduce the amount of assemblies per solution, while starting only specific test types.

### UnitTests

```csharp
public class MyTests
{
  [UnitTest]
  [Fact]
  public void MyTestCase()
  {
    // (...)
  }
}
```

### IntegrationTests

```csharp
public class MyTests
{
  [IntegrationTest]
  [Fact]
  public void MyTestCase()
  {
    // (...)
  }
}
```

### SmokeTests

```csharp
public class MyTests
{
  [SmokeTest]
  [Fact]
  public void MyTestCase()
  {
    // (...)
  }
}
```

### ModuleTests

```csharp
public class MyTests
{
  [ModuleTest]
  [Fact]
  public void MyTestCase()
  {
    // (...)
  }
}
```

### SystemTests

```csharp
public class MyTests
{
  [SystemTest]
  [Fact]
  public void MyTestCase()
  {
    // (...)
  }
}
```

## Conditional test execution

### Execution conditions

#### On

`On` is used to enumerate possible operating systems to execute the test(s) annotated with `SmartFactAttribute`.

```csharp
/// <summary>
/// An enumeration of possible test execution operating systems
/// </summary>
[Flags]
public enum On
{
  /// <summary>
  /// All operating systems
  /// </summary>
  All = 0,

  /// <summary>
  /// Windows only
  /// </summary>
  Windows = 1,

  /// <summary>
  /// Linux only
  /// </summary>
  Linux = 2,

  /// <summary>
  /// MacOS only
  /// </summary>
  MacOS = 4
}
```

#### Execute

`Execute` is used to enumerate possible execution environments for test(s) annotated with `SmartFactAttribute`

```csharp
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
```

### SmartFact

`SmartFactAttribute` is used to annotate XUnit `Fact`(s) which are to be executed conditionally.

```csharp
public class MyTests
{
  [UnitTest]
  [SmartFact(On.Linux)]
  public void MyTestCase()
  {
    // (...)
  }
}
```

```csharp
public class MyTests
{
  [UnitTest]
  [SmartFact(On.Linux, Execute.InAzureDevOps)]
  public void MyTestCase()
  {
    // (...)
  }
}
```

### SmartTheory

`SmartTheoryAttribute` is used to annotate XUnit `Theory`(ies) which are to be executed conditionally.

```csharp
public class MyTests
{
  [UnitTest]
  [SmartTheory(On.Linux)]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  public void MyTestCase(int parameter)
  {
    // (...)
  }
}
```

```csharp
public class MyTests
{
  [UnitTest]
  [SmartTheory(On.Linux, Execute.InAzureDevOps)]
  [InlineData(1)]
  [InlineData(2)]
  [InlineData(3)]
  public void MyTestCase(int parameter)
  {
    // (...)
  }
}
```