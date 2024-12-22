using System.Runtime.CompilerServices;

namespace CloudTek.Build.Tests;

public class SmartBuildAccessor
{
  [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "MainEntryPoint")]
  public static extern ref int GetMain(TestBuild build);
}