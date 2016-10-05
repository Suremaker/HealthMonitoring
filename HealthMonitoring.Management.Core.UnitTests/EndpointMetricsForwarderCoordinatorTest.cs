using System;
using HealthMonitoring.Forwarders;
using HealthMonitoring.Model;
using HealthMonitoring.TimeManagement;
using Moq;
using Xunit;

namespace HealthMonitoring.Management.Core.UnitTests
{
    public class EndpointMetricsForwarderCoordinatorTest
    {
        private readonly Endpoint _endpoint;
        private readonly Mock<IEndpointMetricsForwarder> _forwarder;
        private readonly Mock<IEndpointMetricsForwarder> _duplicateForwarder;

        public EndpointMetricsForwarderCoordinatorTest()
        {
            var endpointId = Guid.NewGuid();
            var health = new EndpointHealth(DateTime.UtcNow, TimeSpan.FromSeconds(1), EndpointStatus.Healthy);
            _endpoint = new Endpoint(Mock.Of<ITimeCoordinator>(), new EndpointIdentity(endpointId, "type", "Address"), new EndpointMetadata("Name", "Group", null));
            _endpoint.UpdateHealth(health);

            _forwarder = new Mock<IEndpointMetricsForwarder>();
            _duplicateForwarder = new Mock<IEndpointMetricsForwarder>();
        }

        [Fact]
        public void EndpointMetricsForwarderCoordinator_should_forward_metrics()
        {
            var coordinator = new EndpointMetricsForwarderCoordinator(new[] { _forwarder.Object });

            coordinator.HandleMetricsForwarding(_endpoint.Identity, _endpoint.Metadata, _endpoint.Health);

            _forwarder.Verify(x => x.ForwardEndpointMetrics(
                It.Is<EndpointDetails>(
                    g => g.EndpointId.Equals(_endpoint.Identity.Id) &&
                    g.Address.Equals(_endpoint.Identity.Address) &&
                    g.Group.Equals(_endpoint.Metadata.Group) &&
                    g.Name.Equals(_endpoint.Metadata.Name) &&
                    g.MonitorType.Equals(_endpoint.Identity.MonitorType)),
                It.Is<EndpointMetrics>(
                    m => m.CheckTimeUtc.Equals(_endpoint.Health.CheckTimeUtc) && 
                    m.ResponseTimeMilliseconds.Equals(_endpoint.Health.ResponseTime.Milliseconds) &&
                    m.Status.Equals(_endpoint.Health.Status.ToString()))),
                Times.Once);
        }

        [Fact]
        public void EndpointMetricsForwarderCoordinator_should_not_have_duplicates_in_forwarders_list()
        {
            var coordinator = new EndpointMetricsForwarderCoordinator(new[] { _forwarder.Object, _duplicateForwarder.Object });

            coordinator.HandleMetricsForwarding(_endpoint.Identity, _endpoint.Metadata, _endpoint.Health);

            _forwarder.Verify(x => x.ForwardEndpointMetrics(
                It.IsAny<EndpointDetails>(),
                It.IsAny<EndpointMetrics>()), Times.Once);

            _duplicateForwarder.Verify(x => x.ForwardEndpointMetrics(
                It.IsAny<EndpointDetails>(),
                It.IsAny<EndpointMetrics>()), Times.Never);
        }
    }
}
