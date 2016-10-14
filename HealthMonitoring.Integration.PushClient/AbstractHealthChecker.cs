using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoring.Integration.PushClient
{
    public abstract class AbstractHealthChecker : IHealthChecker
    {
        public async Task<EndpointHealth> CheckHealthAsync(CancellationToken cancellationToken)
        {
            var details = CaptureDefaultDetails();
            return new EndpointHealth(await OnHealthCheckAsync(details, cancellationToken), details);
        }

        protected abstract Task<HealthStatus> OnHealthCheckAsync(Dictionary<string, string> details, CancellationToken cancellationToken);

        private Dictionary<string, string> CaptureDefaultDetails()
        {
            return new Dictionary<string, string>
            {
                {"Version", GetEndpointVersion()},
                {"Host", GetMachineName()},
                {"Location", GetExecutableFileName()}
            };
        }

        protected virtual string GetMachineName()
        {
            return Environment.MachineName;
        }

        protected virtual string GetExecutableFileName()
        {
            return Assembly.GetEntryAssembly()?.Location;
        }

        protected virtual string GetEndpointVersion()
        {
            return GetType().Assembly.GetName().Version.ToString(4);
        }
    }
}