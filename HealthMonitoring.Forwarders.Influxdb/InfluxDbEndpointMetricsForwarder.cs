using System;

namespace HealthMonitoring.Forwarders.Influxdb
{
    public class InfluxDbEndpointMetricsForwarder : IEndpointMetricsForwarder
    {
        public string Name => "InfluxDbForwarder";
        public void ForwardEndpointMetrics(Guid endpointId, EndpointMetrics metrics)
        {
            //influx code goes here
        }
    }
}
