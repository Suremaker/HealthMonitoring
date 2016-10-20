using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Entities;
using HealthMonitoring.Integration.PushClient;
using HealthMonitoring.Integration.PushClient.Monitoring;
using LightBDD;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    public partial class Push_endpoint_monitoring : FeatureFixture, IDisposable, IHealthChecker
    {
        private Guid _identifier;
        private RestClient _client;
        private string _endpointUniqueName;
        private string _endpointHostName;
        private string _endpointGroupName;
        private string[] _endpointTags;
        private string _endpointName;
        private HealthStatus _currentEndpointStatus = HealthStatus.Offline;
        private readonly Dictionary<string, string> _currentEndpointDetails = new Dictionary<string, string>();
        private HealthMonitorPushClient _pushClient;
        private EndpointEntity _details;
        private IEndpointHealthNotifier _monitor;
        private readonly CredentialsProvider _credentials = new CredentialsProvider();
        private string _password;

        public Push_endpoint_monitoring(ITestOutputHelper output)
            : base(output)
        {
        }

        public void Dispose()
        {
            _monitor?.Dispose();
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void Given_an_endpoint_with_push_integration()
        {
            _endpointUniqueName = Guid.NewGuid().ToString();
            _endpointHostName = Guid.NewGuid().ToString();
            _endpointGroupName = Guid.NewGuid().ToString();
            _endpointName = Guid.NewGuid().ToString();
            _endpointTags = new[] { "tag1", "tag2" };
            _password = Guid.NewGuid().ToString();

            _pushClient = HealthMonitorPushClient.UsingHealthMonitor(ClientHelper.GetHealthMonitorUrl().ToString())
                .DefineEndpoint(b => b
                    .DefineAddress(_endpointHostName, _endpointUniqueName)
                    .DefineGroup(_endpointGroupName)
                    .DefineName(_endpointName)
                    .DefineTags(_endpointTags)
                    .DefinePassword(_password))
                .WithHealthCheck(this);

            _currentEndpointStatus = HealthStatus.Healthy;
        }


        private void When_client_requests_endpoint_details()
        {
            _details = _client.GetEndpointDetails(_identifier);
        }

        private void Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus status)
        {
            _client.EnsureStatusChanged(_identifier, status);
        }

        private void Then_the_endpoint_additional_details_should_be_provided()
        {
            Assert.Equal(_currentEndpointDetails, _details.Details);
        }

        private void When_endpoint_starts()
        {
            _monitor = _pushClient.StartHealthNotifier();
        }

        private void Then_endpoint_should_be_registered_in_Health_Monitor()
        {
            var endpoint = EnsureEndpointRegistered();
            _identifier = endpoint.Id;
        }

        private void Then_the_endpoint_registration_metadata_should_be_provided()
        {
            Assert.Equal(_endpointGroupName, _details.Group);
            Assert.Equal(_endpointName, _details.Name);
            Assert.Equal(_endpointTags, _details.Tags);
            Assert.Equal($"{_endpointHostName}:{_endpointUniqueName}", _details.Address);
        }

        private void Then_monitor_should_receive_updates_within_configured_time_intervals()
        {
            var details = _client.GetEndpointDetails(_identifier);
            var newDetails = Wait.Until(
                Timeouts.Default,
                () => _client.GetEndpointDetails(_identifier),
                det => det.LastCheckUtc != details.LastCheckUtc,
                "No health update received");

            Assert.Equal(details.Status, newDetails.Status);
        }

        private void When_endpoint_health_check_method_detect_health_issues_and_return_unhealthy_status()
        {
            _currentEndpointStatus = HealthStatus.Unhealthy;
        }

        private void Given_endpoint_is_configured_to_provide_additional_details_to_Health_Monitor()
        {
            _currentEndpointDetails.Add("endpointKey", "endpointValue");
        }

        private void When_endpoint_terminates_so_that_it_no_longer_sends_updates()
        {
            _monitor?.Dispose();
            _monitor = null;
        }

        private void When_admin_removes_the_endpoint_from_the_monitor()
        {
            _client
                .Authorize(_credentials.AdminCredentials)
                .Delete(new RestRequest($"/api/endpoints/{_identifier}"))
                .VerifyValidStatus(HttpStatusCode.OK);
        }

        private void Then_endpoint_should_register_itself_again()
        {
            var endpoint = EnsureEndpointRegistered();
            Assert.NotEqual(_identifier, endpoint.Id);
            _identifier = endpoint.Id;
        }

        private EndpointIdentity EnsureEndpointRegistered()
        {
            return Wait.Until(
                Timeouts.Default,
                () => _client.GetEndpointIdentities().FirstOrDefault(e => e.MonitorType == "push" && e.Address == $"{_endpointHostName}:{_endpointUniqueName}"),
                e => e != null,
                "Endpoint was not registered");
        }

        public Task<EndpointHealth> CheckHealthAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new EndpointHealth(_currentEndpointStatus, _currentEndpointDetails));
        }

        private void When_endpoint_health_check_method_detect_health_critical_issues_and_return_faulty_status()
        {
            _currentEndpointStatus = HealthStatus.Faulty;
        }

        private void When_endpoint_health_check_method_detect_endpoint_being_put_offline_and_return_offline_status()
        {
            _currentEndpointStatus = HealthStatus.Offline;
        }
    }
}