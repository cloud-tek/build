using Nuke.Common.IO;

namespace CloudTek.Build.Primitives
{
    public enum TestType
    {
        UnitTests = 0,
        IntegrationTests
    }
    public class Test
    {
        public TestType Type { get; set; }
        public AbsolutePath Project { get; set; } = default!;
    }
}