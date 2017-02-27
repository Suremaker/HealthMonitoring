using System;
using System.Net;
using System.Threading;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Http;
using LightBDD.XUnit2;
using RestSharp;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    public partial class Endpoint_monitoring_with_specific_monitor_process : FeatureFixture, IDisposable
    {
        private Guid _identifier;
        private RestClient _client;
        private MockWebEndpoint _restEndpoint;
        private Tuple<Thread, AppDomain> _monitorProcess;
        private const string CustomMonitorTag = "custom monitor?/:\\tag";

        public Endpoint_monitoring_with_specific_monitor_process(ITestOutputHelper output)
            : base(output)
        {
        }

        public void Dispose()
        {
            if (_restEndpoint != null)
            {
                _restEndpoint.Dispose();
                _restEndpoint = null;
            }

            if (_monitorProcess != null)
                AppDomainExecutor.KillAppDomain(_monitorProcess);
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void Given_a_rest_endpoint()
        {
            _restEndpoint = MockWebEndpointFactory.CreateNew();
        }

        private void Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus status)
        {
            _client.EnsureStatusChanged(_identifier, status);
        }

        private void Given_an_healthy_endpoint_that_has_not_been_deployed_yet()
        {
            Given_a_rest_endpoint();
            _restEndpoint.SetupStatusResponse(HttpStatusCode.OK);
        }

        private void When_client_registers_the_endpoint_with_specific_monitor_tag()
        {
            _identifier = _client.RegisterEndpoint(MonitorTypes.Http, _restEndpoint.StatusAddress, "group", "name", monitorTag: CustomMonitorTag);
        }

        private void Then_monitor_should_observe_endpoint_status_being_STATUS_with_error(EndpointStatus status, string error)
        {
            Wait.Until(Timeouts.Default,
                () => _client.GetEndpointDetails(_identifier),
                e => e.Status == status && e.Details["reason"] == error,
                "Endpoint status did not changed to " + status + " with error: " + error);
        }

        private void When_monitor_process_with_that_tag_starts()
        {
            _monitorProcess = AppDomainExecutor.StartAssembly("monitor2\\HealthMonitoring.Monitors.SelfHost.exe");
        }
    }
}