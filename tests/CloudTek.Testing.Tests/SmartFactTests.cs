using System.Runtime.InteropServices;
using FluentAssertions;

namespace CloudTek.Testing.Tests;

public class SmartFactTests
{
  public class GivenTestMethodIsDecoratedByASmartFact
  {
    [UnitTest]
    [SmartFact(On.Windows)]
    public void WhenExecuteOnWindows_ThenItDoesNotExecuteAnywhereElse()
    {
      RuntimeInformation.IsOSPlatform(OSPlatform.Windows).Should().BeTrue();
    }

    [UnitTest]
    [SmartFact(On.Linux)]
    public void WhenExecuteOnLinux_ThenItDoesNotExecuteAnywhereElse()
    {
      RuntimeInformation.IsOSPlatform(OSPlatform.Linux).Should().BeTrue();
    }

    [UnitTest]
    [SmartFact(On.MacOS)]
    public void WhenExecuteOnMacOs_ThenItDoesNotExecuteAnywhereElse()
    {
      RuntimeInformation.IsOSPlatform(OSPlatform.OSX).Should().BeTrue();
    }
  }
}