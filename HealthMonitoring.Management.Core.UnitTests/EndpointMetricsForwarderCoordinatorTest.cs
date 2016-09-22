using System;
using HealthMonitoring.Forwarders;
using HealthMonitoring.Model;
using Moq;
using Xunit;

namespace HealthMonitoring.Management.Core.UnitTests
{
    public class EndpointMetricsForwarderCoordinatorTest
    {
        [Fact]
        public void EndpointMetricsForwarderCoordinator_should_forward_metrics()
        {
            var forwarderMock = new Mock<IEndpointMetricsForwarder>();

            var endpointId = Guid.NewGuid();
            var health = new EndpointHealth(DateTime.UtcNow, TimeSpan.FromSeconds(1), EndpointStatus.Healthy);

            var coordinator = new EndpointMetricsForwarderCoordinator(new[] { forwarderMock.Object});

            coordinator.HandleMetricsForwarding(endpointId, health);

            forwarderMock.Verify(x => x.ForwardEndpointMetrics(
                It.Is<Guid>(g => g.Equals(endpointId)), 
                It.Is<EndpointMetrics>(m => m.CheckTimeUtc == health.CheckTimeUtc && m.ResponseTimeTicks == health.ResponseTime.Ticks && m.Status == health.Status.ToString())),
                Times.Once);
        }

        [Fact]
        public void EndpointMetricsForwarderCoordinator_shouldnt_have_duplicates_in_forwarders_list()
        {
            var forwarderMock1 = new Mock<IEndpointMetricsForwarder>();
            var forwarderMock2 = new Mock<IEndpointMetricsForwarder>();

            var coordinator = new EndpointMetricsForwarderCoordinator(new[] { forwarderMock1.Object, forwarderMock2.Object });

            coordinator.HandleMetricsForwarding(Guid.NewGuid(), new EndpointHealth(DateTime.UtcNow, TimeSpan.FromSeconds(1), EndpointStatus.Healthy));

            forwarderMock1.Verify(x => x.ForwardEndpointMetrics(It.IsAny<Guid>(), It.IsAny<EndpointMetrics>()), Times.Once);
            forwarderMock2.Verify(x => x.ForwardEndpointMetrics(It.IsAny<Guid>(), It.IsAny<EndpointMetrics>()), Times.Never);
        }
    }
}
