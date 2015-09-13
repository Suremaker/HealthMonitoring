using System.Runtime.CompilerServices;

namespace HealthMonitoring.AcceptanceTests.Helpers
{
    internal static class MockWebEndpointFactory
    {
        private static volatile int _port = 9070;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static MockWebEndpoint CreateNew()
        {
            return new MockWebEndpoint(_port++);
        }
    }
}