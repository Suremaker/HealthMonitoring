using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HealthMonitoring.UnitTests.Helpers
{
    internal static class CollectionAssert
    {
        public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var expectedArray = expected.ToArray();
            var actualArray = actual.ToArray();
            Assert.True(expectedArray.Length == actualArray.Length, string.Format("Collections size are not equal - expected: {0}, got: {1}\nExpected collection: {2}\nActual collection: {3}", expectedArray.Length, actualArray.Length, expectedArray, actualArray));
            var actualList = actualArray.ToList();
            foreach (var expectedItem in expectedArray)
                Assert.True(actualList.Remove(expectedItem), string.Format("Actual collection does not have element: {0}\nExpected collection: {1}\nActual collection: {2}", expectedItem, expectedArray, actualArray));
        }
    }
}
