using System;
using Xunit;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    static class CustomAssertions
    {
        public static void EqualNotStrict(string first, string second)
        {
            bool equal = string.Equals(first, second, StringComparison.CurrentCultureIgnoreCase);
            Assert.True(equal, $"{first} != {second}");
        }
    }
}
