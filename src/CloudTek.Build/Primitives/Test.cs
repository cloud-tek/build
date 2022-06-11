using Nuke.Common.IO;

namespace CloudTek.Build.Primitives
{
    public enum TestType
    {
        UnitTests = 0,
        IntegrationTests,
        ModuleTests,
        E2ETests
    }
    public class Test
    {
        public TestType Type { get; set; }
        public AbsolutePath Project { get; set; } = default!;
    }
}