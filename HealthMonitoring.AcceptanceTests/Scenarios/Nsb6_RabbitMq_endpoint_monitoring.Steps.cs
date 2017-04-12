﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using HealthMonitoring.AcceptanceTests.Helpers;
using HealthMonitoring.AcceptanceTests.Helpers.Entities;
using LightBDD;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RestSharp;
using Xunit;
using Xunit.Abstractions;

namespace HealthMonitoring.AcceptanceTests.Scenarios
{
    public partial class Nsb6_RabbitMq_endpoint_monitoring : FeatureFixture, IDisposable
    {
        private Guid _identifier;
        private RestClient _client;
        private EndpointEntity _details;
        private string _endpointName;
        private Process _process;

        public Nsb6_RabbitMq_endpoint_monitoring(ITestOutputHelper output) : base(output)
        {
        }

        private void Given_a_healthy_nsb_endpoint()
        {
            _process = Process.Start(new ProcessStartInfo("samples\\nsb6rabbitmq\\HealthMonitoring.SampleNsb6Host.RabbitMq.exe") { WindowStyle = ProcessWindowStyle.Hidden });
            _endpointName = "HealthMonitoring.SampleNsb6Host.RabbitMq";
        }

        private void Given_an_endpoint_that_has_not_been_deployed_yet()
        {
            _endpointName = "some_inexistent.localhost";

            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(_endpointName, true, false, false, null);
                channel.ExchangeDeclare(_endpointName, ExchangeType.Fanout, true);
                channel.QueueBind(_endpointName, _endpointName, string.Empty);
            }
        }

        private void Given_an_unreachable_endpoint()
        {
            _endpointName = "unreachable@localhost";
        }

        private void Given_a_monitor_api_client()
        {
            _client = ClientHelper.Build();
        }

        private void When_client_registers_the_endpoint()
        {
            _identifier = _client.RegisterEndpoint(MonitorTypes.Nsb6RabbitMq, _endpointName, "group", "name");
        }

        private void Then_monitor_should_start_monitoring_the_endpoint()
        {
            _client.EnsureMonitoringStarted(_identifier);
        }

        private void When_client_requests_endpoint_details()
        {
            _details = _client.GetEndpointDetails(_identifier);
        }

        private void Then_the_last_check_time_should_be_provided()
        {
            Assert.True(_details.LastCheckUtc != null, "Last check time is not provided");
        }

        private void Then_the_response_time_should_be_provided()
        {
            Assert.True(_details.LastResponseTime != null, "Last response time is not provided");
        }

        private void Then_monitor_should_observe_endpoint_status_being_STATUS(EndpointStatus status)
        {
            _client.EnsureStatusChanged(_identifier, status);
        }

        private void Then_the_endpoint_additional_details_should_contain_timeout_information()
        {
            Assert.Equal(new Dictionary<string, string> { { "message", "health check timeout" } }, _details.Details);
        }

        private void Then_the_endpoint_additional_details_should_contain_failure_information()
        {
            Assert.True(_details.Details.ContainsKey("reason"), "Reason field should be set");
            var reason = _details.Details["reason"];
            Assert.True(Regex.IsMatch(reason, ".+ no exchange \'unreachable@localhost\'.+"), $"Reason field is invalid: {reason}");
        }

        private void Then_the_endpoint_additional_details_should_be_provided()
        {
            Assert.Equal(new Dictionary<string, string> { { "Machine", "localhost" }, { "Version", "1.0.0.0" } }, _details.Details);
        }

        private void Then_the_endpoint_status_should_be_provided(EndpointStatus status)
        {
            Assert.Equal(status, _details.Status);
        }

        public void Dispose()
        {
            _process?.Kill();
        }

        private void When_more_time_pass()
        {
            Thread.Sleep(Timeouts.HealthCheckInterval);
        }
    }
}