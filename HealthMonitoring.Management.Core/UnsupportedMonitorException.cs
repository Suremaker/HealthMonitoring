using System;

namespace HealthMonitoring.Management.Core
{
    public class UnsupportedMonitorException : InvalidOperationException
    {
        public UnsupportedMonitorException(string monitorType):base($"Unsupported monitor: {monitorType}")
        {
        }
    }
}