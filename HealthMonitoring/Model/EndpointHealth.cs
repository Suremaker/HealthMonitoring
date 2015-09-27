using System;
using System.Collections.Generic;
using HealthMonitoring.Monitors;

namespace HealthMonitoring.Model
{
    public class EndpointHealth
    {
        private EndpointHealth(DateTime checkTimeUtc, TimeSpan responseTime, EndpointStatus status, IReadOnlyDictionary<string, string> details)
        {
            CheckTimeUtc = checkTimeUtc;
            ResponseTime = responseTime;
            Status = status;
            Details = details;
        }

        public TimeSpan ResponseTime { get; private set; }
        public EndpointStatus Status { get; private set; }
        public DateTime CheckTimeUtc { get; private set; }
        public IReadOnlyDictionary<string, string> Details { get; private set; }

        public static EndpointHealth FromResult(DateTime checkTimeUtc, HealthInfo health, TimeSpan healthyResponseTimeLimit)
        {
            var status = (EndpointStatus)health.Status;
            if (status == EndpointStatus.Healthy && health.ResponseTime > healthyResponseTimeLimit)
                status = EndpointStatus.Unhealthy;
            return new EndpointHealth(checkTimeUtc, health.ResponseTime, status, health.Details);
        }

        public static EndpointHealth FromException(DateTime checkTimeUtc, Exception exception)
        {
            var details = new Dictionary<string, string>
            {
                {"reason", exception.Message},
                {"exception", exception.ToString()}
            };
            return new EndpointHealth(checkTimeUtc, TimeSpan.Zero, EndpointStatus.Faulty, details);
        }
    }
}