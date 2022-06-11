using CloudTek.Build.Primitives;

namespace CloudTek.Build
{
    public static partial class Constants
    {
        public static class TestCategories
        {
            public static TestType[] CodeCoverageCategories = new[]
            {
                TestType.UnitTests,
                TestType.IntegrationTests
            };
        }
    }
}