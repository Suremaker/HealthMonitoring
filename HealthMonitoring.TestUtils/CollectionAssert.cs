using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace HealthMonitoring.TestUtils
{
    public static class CollectionAssert
    {
        public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var expectedArray = expected.ToArray();
            var actualArray = actual.ToArray();
            Assert.True(expectedArray.Length == actualArray.Length,
                $"Collections size are not equal - expected: {expectedArray.Length}, got: {actualArray.Length}\nExpected collection: {expectedArray}\nActual collection: {actualArray}");
            var actualList = actualArray.ToList();
            foreach (var expectedItem in expectedArray)
                Assert.True(actualList.Remove(expectedItem),
                    $"Actual collection does not have element: {expectedItem}\nExpected collection: {expectedArray}\nActual collection: {actualArray}");
        }
    }
}
